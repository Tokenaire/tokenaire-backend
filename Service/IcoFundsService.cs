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
    }

    public class IcoFundsService : IIcoFundsService
    {
        private readonly string bitGoICOFundsWalletId;
        private readonly IConfiguration configuration;
        private readonly TokenaireContext tokenaireContext;
        private readonly IBitGoService bitGoService;
        private readonly IUserService userService;
        private readonly ISettingsService settingsService;
        private readonly IWavesAssetsNodeService wavesAssetsNodeService;

        private readonly double oneAireInSatoshies = 80;

        public IcoFundsService(
            IConfiguration configuration,
            TokenaireContext tokenaireContext,
            IBitGoService bitGoService,
            IUserService userService,
            ISettingsService settingsService,
            IWavesAssetsNodeService wavesAssetsNodeService)
        {
            this.bitGoICOFundsWalletId = configuration.GetValue<string>("BitGoICOFundsWalletId");
            this.configuration = configuration;
            this.tokenaireContext = tokenaireContext;
            this.bitGoService = bitGoService;
            this.userService = userService;
            this.settingsService = settingsService;
            this.wavesAssetsNodeService = wavesAssetsNodeService;
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
                if (receivedTransfer.Usd >= 1000 && receivedTransfer.Confirmations <= 2) {
                    continue;
                }

                if (receivedTransfer.Usd >= 10000 && receivedTransfer.Confirmations <= 5) {
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
                    if (receivedTransfer.Value < transferEntry.Value) {
                        continue;
                    }

                    var icoOutboundTransaction = icoOutboundTransactions.FirstOrDefault((t) =>
                        t.TxIdSource.ToLower() == receivedTransfer.TxId.ToLower() &&
                        t.AddressSource.ToLower() == transferEntry.Address.ToLower());

                    // we have already processed
                    // the transfer entry.
                    if (icoOutboundTransaction != null)
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

                    // time to actually process the transfer entry.
                    await this.ProcessTransferEntry(user, receivedTransfer, transferEntry);
                }
            }

            return true;
        }

        public async Task<string> GenerateICOBtcAddressForUser(string email)
        {
            if (string.IsNullOrEmpty(email)) {
                return null;
            }

            return await this.bitGoService.GenerateBtcAddress(this.bitGoICOFundsWalletId, $"ICOFUNDS{email}");
        }

        private async Task<bool> ProcessTransferEntry(
            ServiceUser user,
            ServiceBitGoWalletTransfer receivedTransfer,
            ServiceBitGoWalletTransferEntry transferEntry)
        {
            // we will double check if we have already processed these funds or not.
            // BitGO api might give us multiple transfers.
            var icoOutboundTransaction = await this.tokenaireContext
                .ICOOutboundAIRETransactions
                .FirstOrDefaultAsync((t) =>
                        t.TxIdSource.ToLower() == receivedTransfer.TxId.ToLower() &&
                        t.AddressSource.ToLower() == transferEntry.Address.ToLower());

            if (icoOutboundTransaction != null) {
                return false;
            }

            // user didn't buy anything at all,
            // we'll ignore this for now!
            var valueInAIRE = (long)(transferEntry.Value / this.oneAireInSatoshies);
            if (valueInAIRE < 1) {
                return false;
            }

            // add a record that we have seen this wallet transfer entry
            // before.
            var savedIcoOutboundTransaction = new DatabaseIcOOutboundAIRETransaction() {
                TxIdSource = receivedTransfer.TxId,
                AddressSource = transferEntry.Address
            };
            
            await this.tokenaireContext.ICOOutboundAIRETransactions.AddAsync(savedIcoOutboundTransaction);
            await this.tokenaireContext.SaveChangesAsync();

            // we have a record on our database
            // which indicates that we have "TRIED" to send the
            // Waves to their account.
            // If for whatever reason it fails,
            // we will manually need to check it in the future and resolve
            // any problems.
            var wavesAssetTransferResponse = await this.wavesAssetsNodeService.Transfer(new ServiceWavesAssetsNodeTransfer() {
                PrivateKey = this.settingsService.WavesICOAireWalletPrivateKey,
                AssetId = this.configuration.GetValue<string>("AIRETokenAssetId"),
                Fee = 100000,

                Attachment = savedIcoOutboundTransaction.Id.ToString(),
                ToAddress = user.Address,
                Amount = valueInAIRE
            });

            savedIcoOutboundTransaction.IsSuccessful = wavesAssetTransferResponse.IsSuccessful;
            savedIcoOutboundTransaction.Content = wavesAssetTransferResponse.Content;

            await this.tokenaireContext.SaveChangesAsync();
            return wavesAssetTransferResponse.IsSuccessful;
        }
    }
}

