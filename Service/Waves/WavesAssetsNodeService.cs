using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RestSharp;
using Tokenaire.Database;
using Tokenaire.Database.Models;
using Tokenaire.Service.Models;
using WavesCS;

namespace Tokenaire.Service
{
    public interface IWavesAssetsNodeService
    {
        Task<long?> GetBalance(string wavesAddress, string assetId);
        Task<ServiceWavesAssetsNodeTransferResponse> Transfer(ServiceWavesAssetsNodeTransfer transferModel);
    }

    public class WavesAssetsNodeService : IWavesAssetsNodeService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly string _prefix = "assets";

        public WavesAssetsNodeService(IServiceProvider serviceProvider)
       {
            this.serviceProvider = serviceProvider;
        }

        public async Task<long?> GetBalance(string wavesAddress, string assetId) {
            var restSharp = this.serviceProvider.GetService<IWavesNodeRestClientService>();
            var restRequest = new RestRequest(Method.GET);
            restRequest.Resource = $"{this._prefix}/balance/{wavesAddress}/{assetId}";

            var response = await restSharp.ExecuteAsync(restRequest);
            long? balance = null;

            if (response.StatusCode == HttpStatusCode.OK) {
                dynamic content = JsonConvert.DeserializeObject(response.Content);
                balance = content.balance;
            }

            return balance;
        }

        public async Task<ServiceWavesAssetsNodeTransferResponse> Transfer(ServiceWavesAssetsNodeTransfer transferModel) {
            var restSharp = this.serviceProvider.GetService<IWavesNodeRestClientService>();
            var restRequest = new RestRequest(Method.POST);
            restRequest.Resource = $"{this._prefix}/broadcast/transfer";

            var privKeyAcc = PrivateKeyAccount.CreateFromPrivateKey(transferModel.PrivateKey, 'W');
            var transaction = Transactions.MakeTransferTransaction(
                privKeyAcc,
                transferModel.ToAddress,
                transferModel.Amount,
                transferModel.AssetId,
                transferModel.Fee,
                transferModel.FeeAssetId,
                transferModel.Attachment);

            restRequest.AddJsonBody(transaction);

            var response = await restSharp.ExecuteAsync(restRequest);

            return new ServiceWavesAssetsNodeTransferResponse() {
                IsSuccessful = response.StatusCode == HttpStatusCode.OK,
                Content = response.Content
            };
        }
    }
}

