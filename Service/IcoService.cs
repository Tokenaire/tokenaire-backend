using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using org.whispersystems.curve25519;
using RestSharp;
using Tokenaire.Database;
using Tokenaire.Database.Models;
using Tokenaire.Service.Enums;
using Tokenaire.Service.Models;
using Tokenaire.Service.Models.Models;
using WavesCS;



namespace Tokenaire.Service
{
    public interface IIcoService
    {
        Task<string> GenerateICOBtcAddressForUser(string email);
        Task<bool> ProcessFunds();

        Task<long?> GetAIREWalletBalance();

        Task<long> GetAIRELeft();
        Task<long> GetAIRESold();
        Task<ServiceIcoStatus> GetICOStatus();

        Task<ServiceIcoFundsMyDetailsResult> GetMyICODetails(string userId);
        Task<bool> SetRefundAddress(string v, string bTCAddress);
    }

    public class IcoService : IIcoService
    {
        private readonly string bitGoICOFundsWalletId;
        private readonly string bitGoICORefundsWalletId;
        private readonly IConfiguration configuration;
        private readonly TokenaireContext tokenaireContext;
        private readonly IUserReferralLinkService userReferralLinkService;
        private readonly IBitGoService bitGoService;
        private readonly IMemoryCache memoryCache;
        private readonly IIcoKycService icoKycService;
        private readonly IEmailService emailService;
        private readonly IMathService mathService;
        private readonly IUserService userService;
        private readonly ISettingsService settingsService;
        private readonly IWavesAssetsNodeService wavesAssetsNodeService;

        private readonly int referralLinkMinimumRaisedBtc = 1;
        private readonly int referralLinkRateInPercentage = 5;

        private readonly long aireTokenSaleSupply = 4200000000;

        public IcoService(
            IConfiguration configuration,
            TokenaireContext tokenaireContext,
            IUserReferralLinkService userReferralLinkService,
            IBitGoService bitGoService,
            IMemoryCache memoryCache,
            IIcoKycService icoKycService,
            IEmailService emailService,
            IMathService mathService,
            IUserService userService,
            ISettingsService settingsService,
            IWavesAssetsNodeService wavesAssetsNodeService)
        {
            this.bitGoICOFundsWalletId = configuration.GetValue<string>("BitGoICOFundsWalletId");
            this.bitGoICORefundsWalletId = configuration.GetValue<string>("BitGOICORefundsWalletId");

            this.configuration = configuration;
            this.tokenaireContext = tokenaireContext;
            this.userReferralLinkService = userReferralLinkService;
            this.bitGoService = bitGoService;
            this.memoryCache = memoryCache;
            this.icoKycService = icoKycService;
            this.emailService = emailService;
            this.mathService = mathService;
            this.userService = userService;
            this.settingsService = settingsService;
            this.wavesAssetsNodeService = wavesAssetsNodeService;
        }

        public async Task<bool> SetRefundAddress(string userId, string BTCAddress)
        {
            var user = await this.tokenaireContext.Users.FirstAsync(x => x.Id == userId);
            if (user.ICOBTCAddress == user.ICOBTCRefundAddress)
            {
                throw new InvalidOperationException("not allowed");
            }

            user.ICOBTCRefundAddress = BTCAddress;
            await this.tokenaireContext.SaveChangesAsync();
            return true;
        }

        public async Task<long> GetAIRESold()
        {
            return await this.tokenaireContext.ICOTransactions
                .Select(x => x.ValueSentInAIRE)
                .SumAsync();
        }

        public async Task<long> GetAIRELeft()
        {
            return this.aireTokenSaleSupply - await this.GetAIRESold();
        }

        public async Task<ServiceIcoStatus> GetICOStatus()
        {
            var icoStatus = ServiceIcoStatus.NotYetStarted;
            if (DateTime.UtcNow > new DateTime(2018, 07, 14, 10, 0, 0)) {
                icoStatus = ServiceIcoStatus.Running;
            }

            if (DateTime.UtcNow >= new DateTime(2018, 10, 31, 10, 0, 0)) {
                icoStatus = ServiceIcoStatus.Finished;
            }

            if (icoStatus == ServiceIcoStatus.Running)
            {
                if (await this.GetAIRELeft() < 1000000)
                {
                    return ServiceIcoStatus.Finished;
                }

                return ServiceIcoStatus.Running;
            }

            return icoStatus;
        }

        public async Task<long?> GetAIREWalletBalance()
        {
            var cacheKey = "AIREBALANCE";
            var assetId = this.configuration.GetValue<string>("AIRETokenAssetId");
            var wavesAddress = this.configuration.GetValue<string>("WavesAddress");

            // if we find aire balance from the cache
            // we will just return that.

            string cachedAIREBalance = null;
            if (this.memoryCache.TryGetValue(cacheKey, out cachedAIREBalance))
            {
                return cachedAIREBalance != null ? long.Parse(cachedAIREBalance) : (long?)null;
            }

            // if not,
            // we'll just have to query the aire balance ourselves from waves api!
            var AIREBalance = await this.wavesAssetsNodeService.GetBalance(wavesAddress, assetId);
            this.memoryCache.Set<string>(cacheKey, AIREBalance?.ToString(), new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(10),
                Priority = CacheItemPriority.Normal
            });

            return AIREBalance;
        }

        public async Task<bool> ProcessFunds()
        {
            var transfers = await this.bitGoService.GetWalletTransfers(this.bitGoICOFundsWalletId);
            var receivedTransfers = transfers.Where((transfer) =>
                transfer.State == "confirmed" &&
                transfer.Confirmations >= 1 &&
                transfer.Value >= 0);

            var allUsers = await this.userService.GetUsers();

            foreach (var receivedTransfer in receivedTransfers)
            {
                // we'll just ensure the confirmations and USD size match
                // more or less.
                // the more USD we get, the more we wait for confirmation
                // before sending the funds over.
                if (receivedTransfer.Usd >= 1000 && receivedTransfer.Confirmations <= 2)
                {
                    continue;
                }

                if (receivedTransfer.Usd >= 10000 && receivedTransfer.Confirmations <= 5)
                {
                    continue;
                }

                foreach (var transferEntry in receivedTransfer.Entries)
                {
                    // entry has no value or is negative,
                    // we don't care about this.
                    if (transferEntry.Value <= 0)
                    {
                        continue;
                    }

                    // this can't happen in my opinion,
                    // but just a sanity check.
                    if (receivedTransfer.Value < transferEntry.Value)
                    {
                        continue;
                    }

                    var user = allUsers.FirstOrDefault(u =>
                        u.ICOBTCAddress != null &&
                        u.ICOBTCAddress.ToLower() == transferEntry.Address.ToLower());

                    // seems like address doesn't belong to anyone,
                    // we will just ditch it then!
                    if (user == null)
                    {
                        continue;
                    }

                    var icoOutboundTransaction = await this.tokenaireContext
                        .ICOTransactions
                        .FirstOrDefaultAsync((t) =>
                            t.TxIdSource.ToLower() == receivedTransfer.TxId.ToLower() &&
                            t.ICOBTCAddress.ToLower() == transferEntry.Address.ToLower());

                    if (icoOutboundTransaction != null)
                    {
                        await this.ProcessExistingIcoTransaction(user, receivedTransfer, transferEntry, icoOutboundTransaction);
                        continue;
                    }

                    if (icoOutboundTransaction == null)
                    {
                        await this.CreateNewIcoTransaction(user, receivedTransfer, transferEntry);
                    }
                }
            }

            return true;
        }

        public async Task<string> GenerateICOBtcAddressForUser(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            return await this.bitGoService.GenerateBtcAddress(this.bitGoICOFundsWalletId, $"ICOFUNDS{email}");
        }

        public async Task<ServiceIcoFundsMyDetailsResult> GetMyICODetails(string userId)
        {
            var user = await this.userService.GetUser(userId);
            var referralLinkDetails = await this.GetReferralLinkDetails(userId);
            var ICOBTCInvestedSatoshies = await this.tokenaireContext.ICOTransactions
                .Where(x => x.UserId == userId)
                .Select(x => x.ValueReceivedInSatoshies)
                .SumAsync();

            var AIREToReceive = await this.tokenaireContext.ICOTransactions
                .Where(x => x.UserId == userId)
                .Select(x => x.ValueSentInAIRE)
                .SumAsync();

            (var oneAirePriceInSatoshies, var discountRate) = this.GetOneAIREPriceInSatoshies(user.RegisteredFromReferralLinkId);

            return new ServiceIcoFundsMyDetailsResult()
            {
                IsKyced = user.ICOKyced,

                ICOBTCAddress = user.ICOBTCAddress,
                ICOBTCRefundAddress = user.ICOBTCRefundAddress,
                ICOBTCInvestedSatoshies = ICOBTCInvestedSatoshies,
                ReferralLinkUrl = referralLinkDetails.ReferralLinkUrl,
                ReferralLinkUsedByPeople = referralLinkDetails.ReferralLinkUsedByPeople,
                ReferralLinkRate = this.referralLinkRateInPercentage,
                ReferralLinkRaisedBtcSatoshies = referralLinkDetails.ReferralLinkRaisedBtcSatoshies,
                ReferralLinkEligibleBtcSatoshies = referralLinkDetails.ReferralLinkEligibleBtcSatoshies,

                AIREToReceive = AIREToReceive,

                OneAireInSatoshies = oneAirePriceInSatoshies,
                DiscountRate = discountRate
            };
        }

        private async Task<(long ReferralLinkRaisedBtcSatoshies, long ReferralLinkEligibleBtcSatoshies, string ReferralLinkUrl, int ReferralLinkUsedByPeople)> GetReferralLinkDetails(string userId)
        {
            var platformUrl = this.configuration.GetValue<string>("TokenairePlatformUrl");
            var referralLinkId = await this
                .userReferralLinkService
                .GetReferralLinkIdAsync(userId, ServiceUserReferralLinkType.ICO);

            if (string.IsNullOrEmpty(referralLinkId))
            {
                throw new InvalidOperationException("not possible");
            }

            var referralLinkRaisedBtcSatoshies = await this.tokenaireContext.ICOTransactions
                .Where(x => x.RegisteredFromReferralLinkId == referralLinkId)
                .Select(x => x.ValueReceivedInSatoshies)
                .SumAsync();

            var referralLinkUsedByPeople = await this.tokenaireContext.Users
                .Where(x => x.RegistrationInfo.RegisteredFromReferralLinkId == referralLinkId)
                .CountAsync();

            var referralLinkUrl = $"{platformUrl}/?referralLinkId={referralLinkId}";
            var referralLinkEligibleBtcSatoshies = referralLinkRaisedBtcSatoshies / 100 * this.referralLinkRateInPercentage;
            if (this.mathService.ConvertSatoshiesToBTC(referralLinkRaisedBtcSatoshies) < this.referralLinkMinimumRaisedBtc)
            {
                referralLinkEligibleBtcSatoshies = 0;
            }

            return (
                referralLinkRaisedBtcSatoshies,
                referralLinkEligibleBtcSatoshies,
                referralLinkUrl,
                referralLinkUsedByPeople);
        }

        private async Task<bool> ProcessExistingIcoTransaction(
            ServiceUser user,
            ServiceBitGoWalletTransfer receivedTransfer,
            ServiceBitGoWalletTransferEntry transferEntry,
            DatabaseIcoTransaction icoTransaction)
        {
            if (string.IsNullOrEmpty(icoTransaction.ProcessType))
            {
                return false;
            }

            if (icoTransaction.IsProcessed)
            {
                return false;
            }

            if (!user.ICOKyced) {
                return false;
            }

            icoTransaction.IsProcessed = true;
            await this.tokenaireContext.SaveChangesAsync();
            await this.tokenaireContext.ICOTransactionsHistory.AddAsync(new DatabaseIcoTransactionProcessHistory()
            {
                IcoTransactionId = icoTransaction.Id,
                Content = JsonConvert.SerializeObject(new
                {
                    Message = "starting to process an ico transaction",
                    Date = DateTime.UtcNow,
                    Type = icoTransaction.ProcessType,
                    Snapshot = this.GetIcoTransactionSnapshot(user, icoTransaction)
                })
            });
            await this.tokenaireContext.SaveChangesAsync();

            if (icoTransaction.ProcessType == "send_aire")
            {
                var wavesAssetTransferResponse = await this.wavesAssetsNodeService.Transfer(new ServiceWavesAssetsNodeTransfer()
                {
                    PrivateKey = this.settingsService.WavesICOAireWalletPrivateKey,
                    AssetId = this.configuration.GetValue<string>("AIRETokenAssetId"),
                    Fee = 100000,

                    Attachment = icoTransaction.Id.ToString(),
                    ToAddress = user.Address,
                    Amount = icoTransaction.ValueSentInAIRE
                });

                icoTransaction.IsSuccessful = wavesAssetTransferResponse.IsSuccessful;
                icoTransaction.Content = wavesAssetTransferResponse.Content;

                await this.tokenaireContext.SaveChangesAsync();
                await this.tokenaireContext.ICOTransactionsHistory.AddAsync(new DatabaseIcoTransactionProcessHistory()
                {
                    IcoTransactionId = icoTransaction.Id,
                    Content = JsonConvert.SerializeObject(new
                    {
                        Message = "processed aire sending",
                        Date = DateTime.UtcNow,
                        Type = icoTransaction.ProcessType,
                        Snapshot = this.GetIcoTransactionSnapshot(user, icoTransaction)
                    })
                });
                await this.tokenaireContext.SaveChangesAsync();

                // if we sent AIRE tokens to user successfully,
                // we'll notify him with emails.
                if (icoTransaction.IsSuccessful == true)
                {
                    var marketPlaceLink = this.configuration.GetValue<string>("TokenairePlatformUrl");
                    await this.emailService.SendSingleEmailUsingTemplate(new ServiceEmailSendUsingTemplate()
                    {
                        TemplateId = ServiceEmailTemplateEnum.AireSentSuccessful,
                        ToEmail = user.Email,

                        Substitutions = new List<ServiceEmailSendUsingTemplateSubstitution>() {
                            new ServiceEmailSendUsingTemplateSubstitution("MARKETPLACE_LINK", marketPlaceLink),
                            new ServiceEmailSendUsingTemplateSubstitution("VALUERECEIVED_SATOSHIES", icoTransaction.ValueReceivedInSatoshies.ToString()),
                            new ServiceEmailSendUsingTemplateSubstitution("VALUESENT_INAIRE", icoTransaction.ValueSentInAIRE.ToString())
                        }
                    });
                }

                return wavesAssetTransferResponse.IsSuccessful;
            }

            throw new NotSupportedException("processtype" + icoTransaction.ProcessType + " not supported");
        }

        private async Task<bool> CreateNewIcoTransaction(
            ServiceUser user,
            ServiceBitGoWalletTransfer receivedTransfer,
            ServiceBitGoWalletTransferEntry transferEntry)
        {
            (var oneAirePriceInSatoshies, var discountRate) = this.GetOneAIREPriceInSatoshies(user.RegisteredFromReferralLinkId);
            var valueInAIRE = (long)(transferEntry.Value / oneAirePriceInSatoshies);
            if (valueInAIRE < 1)
            {
                return false;
            }

            var marketPlaceLink = this.configuration.GetValue<string>("TokenairePlatformUrl");
            var icoTransaction = new DatabaseIcoTransaction()
            {
                TxIdSource = receivedTransfer.TxId,
                ICOBTCAddress = transferEntry.Address,

                ValueReceivedInSatoshies = transferEntry.Value,
                ValueSentInAIRE = valueInAIRE,

                OneAirePriceInSatoshies = oneAirePriceInSatoshies,

                RegisteredFromReferralLinkId = user.RegisteredFromReferralLinkId,
                UserId = user.Id,

                IsProcessed = false,
                ProcessType = null
            };

            await this.tokenaireContext.ICOTransactions.AddAsync(icoTransaction);
            await this.tokenaireContext.SaveChangesAsync();
            await this.tokenaireContext.ICOTransactionsHistory.AddAsync(new DatabaseIcoTransactionProcessHistory()
            {
                IcoTransactionId = icoTransaction.Id,
                Content = JsonConvert.SerializeObject(new
                {
                    Message = "created new ico transaction",
                    Date = DateTime.UtcNow,
                    Type = icoTransaction.ProcessType,
                    Snapshot = this.GetIcoTransactionSnapshot(user, icoTransaction)
                })
            });
            await this.tokenaireContext.SaveChangesAsync();
            await this.emailService.SendSingleEmailUsingTemplate(new ServiceEmailSendUsingTemplate()
            {
                TemplateId = ServiceEmailTemplateEnum.AirePurchaseSuccessful,
                ToEmail = user.Email,

                Substitutions = new List<ServiceEmailSendUsingTemplateSubstitution>() {
                    new ServiceEmailSendUsingTemplateSubstitution("MARKETPLACE_LINK", marketPlaceLink),
                    new ServiceEmailSendUsingTemplateSubstitution("VALUERECEIVED_SATOSHIES", icoTransaction.ValueReceivedInSatoshies.ToString()),
                    new ServiceEmailSendUsingTemplateSubstitution("VALUESENT_INAIRE", icoTransaction.ValueSentInAIRE.ToString())
                }
            });
            return true;
        }

        private object GetIcoTransactionSnapshot(ServiceUser user, DatabaseIcoTransaction icoTransaction)
        {
            return new
            {
                TxIdSource = icoTransaction.TxIdSource,
                ICOBTCAddress = icoTransaction.ICOBTCAddress,

                ValueReceivedInSatoshies = icoTransaction.ValueReceivedInSatoshies,
                ValueSentInAIRE = icoTransaction.ValueSentInAIRE,

                OneAirePriceInSatoshies = icoTransaction.OneAirePriceInSatoshies,

                RegisteredFromReferralLinkId = icoTransaction.RegisteredFromReferralLink,
                UserId = icoTransaction.UserId,

                IsProcessed = icoTransaction.IsProcessed,
                ProcessType = icoTransaction.ProcessType
            };
        }

        private (double OneAirePriceInSatoshies, double DiscountRate) GetOneAIREPriceInSatoshies(string registeredFromReferralLinkId)
        {
            var curTime = DateTime.UtcNow;
            var isEarlyBirdRound = curTime >= new DateTime(2018, 07, 14, 10, 0, 0) && 
                curTime <= new DateTime(2018, 08, 31, 23, 59, 59);

            var isPresaleRound = curTime >= new DateTime(2018, 09, 01, 0, 0, 0) && 
                curTime <= new DateTime(2018, 09, 30, 10, 0, 0);

            var oneAireInSatoshiesNormal = 4;
            var oneAireInSatoshiesEarlyBird = oneAireInSatoshiesNormal / 1.2;
            var oneAireInSatoshiesPresale = oneAireInSatoshiesNormal / 1.1;
            var oneAireInSatoshies = (double) oneAireInSatoshiesNormal;

            if (isEarlyBirdRound || isPresaleRound) {
                oneAireInSatoshies = isEarlyBirdRound ? oneAireInSatoshiesEarlyBird : oneAireInSatoshiesPresale;
            }

            var oneAireInSatoshiesRegisteredFromReferralLink = oneAireInSatoshies / 1.05;

            var oneAireInSatoshiesFinal = Math.Round(
                string.IsNullOrEmpty(registeredFromReferralLinkId) ?
                    oneAireInSatoshies :
                    oneAireInSatoshiesRegisteredFromReferralLink,
                1);

            var discountRate = Math.Round((oneAireInSatoshiesNormal - oneAireInSatoshiesFinal) / oneAireInSatoshiesNormal * 100, 1);

            return (
                oneAireInSatoshiesFinal,
                discountRate
            );
        }
    }
}

