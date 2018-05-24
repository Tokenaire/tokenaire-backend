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
using Tokenaire.Service.Models;
using Tokenaire.Service.Models.Models;
using WavesCS;

namespace Tokenaire.Service
{
    public interface IIcoFundsService
    {
        Task<string> GenerateICOBtcAddressForUser(string email);
        Task<bool> ProcessFunds();
        Task<long?> GetAIREWalletBalance();

        Task<long> GetAIRELeft();
        Task<long> GetAIRESold();

        Task<ServiceIcoFundsMyDetailsResult> GetMyICODetails(string userId);
    }

    public class IcoFundsService : IIcoFundsService
    {
        private readonly string bitGoICOFundsWalletId;
        private readonly IConfiguration configuration;
        private readonly TokenaireContext tokenaireContext;
        private readonly IUserReferralLinkService userReferralLinkService;
        private readonly IBitGoService bitGoService;
        private readonly IMemoryCache memoryCache;
        private readonly IMathService mathService;
        private readonly IUserService userService;
        private readonly ISettingsService settingsService;
        private readonly IWavesAssetsNodeService wavesAssetsNodeService;

        private readonly int referralLinkMinimumRaisedBtc = 1;
        private readonly int referralLinkRateInPercentage = 5;

        private readonly long aireTokenSaleSupply = 4200000000;

        private readonly bool canSendAIRETokens = false;

        public IcoFundsService(
            IConfiguration configuration,
            TokenaireContext tokenaireContext,
            IUserReferralLinkService userReferralLinkService,
            IBitGoService bitGoService,
            IMemoryCache memoryCache,
            IMathService mathService,
            IUserService userService,
            ISettingsService settingsService,
            IWavesAssetsNodeService wavesAssetsNodeService)
        {
            this.bitGoICOFundsWalletId = configuration.GetValue<string>("BitGoICOFundsWalletId");
            this.configuration = configuration;
            this.tokenaireContext = tokenaireContext;
            this.userReferralLinkService = userReferralLinkService;
            this.bitGoService = bitGoService;
            this.memoryCache = memoryCache;
            this.mathService = mathService;
            this.userService = userService;
            this.settingsService = settingsService;
            this.wavesAssetsNodeService = wavesAssetsNodeService;
        }

        public async Task<long> GetAIRESold()
        {
            return await this.tokenaireContext.ICOOutboundAIRETransactions
                .Select(x => x.ValueSentInAIRE)
                .SumAsync();
        }

        public async Task<long> GetAIRELeft() {
            return this.aireTokenSaleSupply - await this.GetAIRESold();
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
            var icoOutboundTransactions = await this.tokenaireContext.ICOOutboundAIRETransactions.ToListAsync();

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

                    var icoOutboundTransaction = icoOutboundTransactions.FirstOrDefault((t) =>
                        t.TxIdSource.ToLower() == receivedTransfer.TxId.ToLower() &&
                        t.AddressSource.ToLower() == transferEntry.Address.ToLower());

                    if (icoOutboundTransaction != null)
                    {
                        await this.ProcessExistingTransferEntry(user, receivedTransfer, transferEntry, icoOutboundTransaction);
                        continue;
                    }

                    if (icoOutboundTransaction == null)
                    {
                        await this.ProcessNewTransferEntry(user, receivedTransfer, transferEntry);
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
            var ICOBTCInvestedSatoshies = await this.tokenaireContext.ICOOutboundAIRETransactions
                .Where(x => x.UserId == userId)
                .Select(x => x.ValueReceivedInSatoshies)
                .SumAsync();

            return new ServiceIcoFundsMyDetailsResult()
            {
                ICOBTCAddress = user.ICOBTCAddress,
                ICOBTCInvestedSatoshies = ICOBTCInvestedSatoshies,
                ReferralLinkUrl = referralLinkDetails.ReferralLinkUrl,
                ReferralLinkRate = this.referralLinkRateInPercentage,
                ReferralLinkRaisedBtcSatoshies = referralLinkDetails.ReferralLinkRaisedBtcSatoshies,
                ReferralLinkEligibleBtcSatoshies = referralLinkDetails.ReferralLinkEligibleBtcSatoshies,

                OneAireInSatoshies = this.GetOneAIREPriceInSatoshies(user.RegisteredFromReferralLinkId)
            };
        }

        private async Task<(long ReferralLinkRaisedBtcSatoshies, long ReferralLinkEligibleBtcSatoshies, string ReferralLinkUrl)> GetReferralLinkDetails(string userId)
        {
            var platformUrl = this.configuration.GetValue<string>("TokenairePlatformUrl");
            var referralLinkId = await this
                .userReferralLinkService
                .GetReferralLinkIdAsync(userId, ServiceUserReferralLinkType.ICO);

            if (string.IsNullOrEmpty(referralLinkId))
            {
                throw new InvalidOperationException("not possible");
            }

            var referralLinkRaisedBtcSatoshies = await this.tokenaireContext.ICOOutboundAIRETransactions
                .Where(x => x.RegisteredFromReferralLinkId == referralLinkId)
                .Select(x => x.ValueReceivedInSatoshies)
                .SumAsync();

            var referralLinkUrl = $"{platformUrl}/?referralLinkId={referralLinkId}";
            var referralLinkEligibleBtcSatoshies = referralLinkRaisedBtcSatoshies / 100 * this.referralLinkRateInPercentage;
            if (this.mathService.ConvertSatoshiesToBTC(referralLinkRaisedBtcSatoshies) < this.referralLinkMinimumRaisedBtc)
            {
                referralLinkEligibleBtcSatoshies = 0;
            }

            return (referralLinkRaisedBtcSatoshies, referralLinkEligibleBtcSatoshies, referralLinkUrl);
        }

        private async Task<bool> ProcessExistingTransferEntry(
            ServiceUser user,
            ServiceBitGoWalletTransfer receivedTransfer,
            ServiceBitGoWalletTransferEntry transferEntry,
            DatabaseIcOOutboundAIRETransaction savedIcoOutboundTransaction)
        {

            if (!this.canSendAIRETokens)
            {
                return false;
            }

            if (savedIcoOutboundTransaction.IsProcessed)
            {
                return false;
            }

            savedIcoOutboundTransaction.IsProcessed = true;
            await this.tokenaireContext.SaveChangesAsync();

            // we have a record on our database
            // which indicates that we have "TRIED" to send the
            // Waves to their account.
            // If for whatever reason it fails,
            // we will manually need to check it in the future and resolve
            // any problems.
            var wavesAssetTransferResponse = await this.wavesAssetsNodeService.Transfer(new ServiceWavesAssetsNodeTransfer()
            {
                PrivateKey = this.settingsService.WavesICOAireWalletPrivateKey,
                AssetId = this.configuration.GetValue<string>("AIRETokenAssetId"),
                Fee = 100000,

                Attachment = savedIcoOutboundTransaction.Id.ToString(),
                ToAddress = user.Address,
                Amount = savedIcoOutboundTransaction.ValueSentInAIRE
            });

            savedIcoOutboundTransaction.IsSuccessful = wavesAssetTransferResponse.IsSuccessful;
            savedIcoOutboundTransaction.Content = wavesAssetTransferResponse.Content;

            await this.tokenaireContext.SaveChangesAsync();
            return wavesAssetTransferResponse.IsSuccessful;
        }

        private async Task<bool> ProcessNewTransferEntry(
            ServiceUser user,
            ServiceBitGoWalletTransfer receivedTransfer,
            ServiceBitGoWalletTransferEntry transferEntry)
        {
            var oneAirePriceInSatoshies = this.GetOneAIREPriceInSatoshies(user.RegisteredFromReferralLinkId);
            var valueInAIRE = (long)(transferEntry.Value / oneAirePriceInSatoshies);
            if (valueInAIRE < 1)
            {
                return false;
            }

            // add a record that we have seen this wallet transfer entry
            // before.
            var savedIcoOutboundTransaction = new DatabaseIcOOutboundAIRETransaction()
            {
                TxIdSource = receivedTransfer.TxId,
                AddressSource = transferEntry.Address,

                ValueReceivedInSatoshies = transferEntry.Value,
                ValueSentInAIRE = valueInAIRE,

                OneAirePriceInSatoshies = oneAirePriceInSatoshies,

                RegisteredFromReferralLinkId = user.RegisteredFromReferralLinkId,
                UserId = user.Id
            };

            await this.tokenaireContext.ICOOutboundAIRETransactions.AddAsync(savedIcoOutboundTransaction);
            await this.tokenaireContext.SaveChangesAsync();
            return true;
        }

        private long GetOneAIREPriceInSatoshies(string registeredFromReferralLinkId)
        {
            var oneAireInSatoshiesNormal = 80;
            var oneAireInSatoshiesReferralLink = 60;
            return string.IsNullOrEmpty(registeredFromReferralLinkId) ?
                oneAireInSatoshiesNormal :
                oneAireInSatoshiesReferralLink;
        }
    }
}

