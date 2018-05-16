using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using Tokenaire.Database;
using Tokenaire.Database.Models;
using Tokenaire.Service.Models;

namespace Tokenaire.Service
{
    public interface IWavesNodeRestClientService : IRestClient
    {
        Task<RestResponse> ExecuteAsync(RestRequest request);
    }

    public class WavesNodeRestClientService : RestClient, IWavesNodeRestClientService
    {
        private readonly string ApiKey;

        public WavesNodeRestClientService(IConfiguration configuration, ISettingsService settingsService)
        {
            BaseUrl = new Uri(configuration.GetValue<string>("WavesNodeApiUrl"));
            ApiKey = settingsService.WavesNodeApiKey;
        }

        public async Task<RestResponse> ExecuteAsync(RestRequest request)
        {
            request.AddHeader("X-API-Key", ApiKey);

            TaskCompletionSource<IRestResponse> taskCompletion = new TaskCompletionSource<IRestResponse>();
            RestRequestAsyncHandle handle = this.ExecuteAsync(request, r => taskCompletion.SetResult(r));
            return (RestResponse)(await taskCompletion.Task);
        }
    }
}

