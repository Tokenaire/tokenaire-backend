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
    public interface IBitGoService
    {
        Task<string> GenerateBtcAddress(string walletAddress, string label);
        Task<List<ServiceBitGoWalletTransfer>> GetWalletTransfers(string walletAddress);
    }

    public class BitGoService : IBitGoService
    {
        private readonly Encoding U8 = Encoding.UTF8;
        private readonly string apiKey;
        private readonly string apiUrl;
        private readonly string bitGoICOFundsWalletId;

        public BitGoService(IConfiguration configuration, ISettingsService settingsService)
        {
            apiKey = settingsService.BitGoICOFundsApiKey;
            apiUrl = configuration.GetValue<string>("BitGoApiUrl");
            bitGoICOFundsWalletId = configuration.GetValue<string>("BitGoICOFundsWalletId");
        }

        public async Task<List<ServiceBitGoWalletTransfer>> GetWalletTransfers(string walletAddress)
        {
            var running = true;
            var prevId = "";
            var serviceWalletTransfers = new List<ServiceBitGoWalletTransfer>();

            while (running)
            {
                var url = $"btc/wallet/{walletAddress}/transfer";
                var queryParameters = new List<KeyValuePair<string, string>>();
                if (!string.IsNullOrEmpty(prevId))
                {
                    queryParameters.Add(new KeyValuePair<string, string>("prevId", prevId));
                }

                var response = await this.getAsync(url, queryParameters);

                if (response.StatusCode == HttpStatusCode.OK)
                {

                    var walletTransfersModel = JsonConvert.DeserializeObject<WalletTransfers>(response.Content);
                    foreach (var walletTransfer in walletTransfersModel.Transfers)
                    {
                        serviceWalletTransfers.Add(new ServiceBitGoWalletTransfer()
                        {
                            TxId = walletTransfer.TxId,
                            Confirmations = walletTransfer.Confirmations,
                            State = walletTransfer.State,
                            Value = walletTransfer.Value,

                            Usd = walletTransfer.Usd,

                            Entries = walletTransfer.Entries.Select((e) => new ServiceBitGoWalletTransferEntry()
                            {
                                Address = e.Address,
                                Wallet = e.Wallet,
                                Value = e.Value
                            }).ToList()
                        });
                    }

                    running = !string.IsNullOrEmpty(walletTransfersModel.NextBatchPrevId);
                    prevId = walletTransfersModel.NextBatchPrevId;
                }
                else
                {
                    // not expecting this
                    // to really happen,
                    // however everything is possible
                    throw new Exception($"Failed to get wallet transfers" + response.Content);
                }
            }

            return serviceWalletTransfers;
        }

        public async Task<string> GenerateBtcAddress(string walletAddress, string label)
        {
            var url = $"btc/wallet/{walletAddress}/address";
            var response = await this.postAsync(url, new
            {
                label = label
            });

            if (response.StatusCode == HttpStatusCode.OK)
            {
                dynamic data = JsonConvert.DeserializeObject(response.Content);
                return data.address;
            }
            else
            {
                // not expecting this
                // to really happen,
                // however everything is possible
                throw new Exception($"Failed to generate BTC address: {label}");
            }
        }

        private async Task<RestResponse> postAsync(string endpoint, dynamic body)
        {
            var client = new RestClient(this.apiUrl);
            var request = new RestRequest(endpoint, Method.POST);

            request.AddHeader("Authorization", $"Bearer {apiKey}");
            request.AddJsonBody(body);

            TaskCompletionSource<IRestResponse> taskCompletion = new TaskCompletionSource<IRestResponse>();
            RestRequestAsyncHandle handle = client.ExecuteAsync(request, r => taskCompletion.SetResult(r));
            return (RestResponse)(await taskCompletion.Task);
        }

        private async Task<RestResponse> getAsync(string endpoint, List<KeyValuePair<string, string>> queryParameters)
        {
            var client = new RestClient(this.apiUrl);
            var request = new RestRequest(endpoint, Method.GET);

            foreach (var pair in queryParameters)
            {
                request.AddQueryParameter(pair.Key, pair.Value);
            }

            request.AddHeader("Authorization", $"Bearer {apiKey}");

            TaskCompletionSource<IRestResponse> taskCompletion = new TaskCompletionSource<IRestResponse>();
            RestRequestAsyncHandle handle = client.ExecuteAsync(request, r => taskCompletion.SetResult(r));
            return (RestResponse)(await taskCompletion.Task);
        }

        public class WalletTransfers
        {
            public List<WalletTransfer> Transfers { get; set; }
            public string NextBatchPrevId { get; set; }
        }

        public class WalletTransfer
        {
            public string TxId { get; set; }
            public string State { get; set; }

            public long Value { get; set; }
            public long Confirmations { get; set; }

            public double Usd { get; set; }

            public List<WalletTransferEntry> Entries { get; set; }
        }

        public class WalletTransferEntry
        {
            public string Address { get; set; }
            public string Wallet { get; set; }

            public long Value { get; set; }
        }
    }
}

