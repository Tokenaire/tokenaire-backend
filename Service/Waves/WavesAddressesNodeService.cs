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

namespace Tokenaire.Service
{
    public interface IWavesAddressesNodeService
    {
        Task<string> GetAddressFromPublicKey(string publicKey);
    }

    public class WavesAddressesNodeService : IWavesAddressesNodeService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly string _prefix = "addresses";

        public WavesAddressesNodeService(IServiceProvider serviceProvider)
       {
            this.serviceProvider = serviceProvider;
        }

        public async Task<string> GetAddressFromPublicKey(string publicKey) {
            var restSharp = this.serviceProvider.GetService<IWavesNodeRestClientService>();
            var restRequest = new RestRequest(Method.GET);
            restRequest.Resource = $"{this._prefix}/publicKey/{publicKey}";

            var response = await restSharp.ExecuteAsync(restRequest);
            if (response.StatusCode == HttpStatusCode.OK) {
                dynamic data = JsonConvert.DeserializeObject(response.Content);
                return data.address;
            }

            return "";
        }

        public class WavesAddressesNodeServiceResponse {}
    }
}

