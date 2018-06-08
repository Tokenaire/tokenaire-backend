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
    public interface ISumSubApiService
    {
        Task<string> CreateIFrameAccessToken(string userId);
    }

    public class SumSubApiService : ISumSubApiService
    {
        private readonly string apiKey;
        private readonly string apiUrl;
        private readonly ILogService logService;

        public SumSubApiService(IConfiguration configuration, ISettingsService settingsService, ILogService logService)
        {
            apiKey = settingsService.SumSubApiKey;
            apiUrl = settingsService.SumSubApiUrl;
            this.logService = logService;
        }

        public async Task<string> CreateIFrameAccessToken(string userId)
        {
            var response = await this.postAsync($"resources/accessTokens/?userId={userId}&key={apiKey}",
                new { });

            if (response.StatusCode == HttpStatusCode.OK)
            {
                dynamic content = JsonConvert.DeserializeObject(response.Content);
                return content.token;
            }

            await this.logService.Error(new ServiceLogError()
            {
                Message = "Unable to create iframe access token for KYC" + response.Content
            });
            return null;
        }

        private async Task<RestResponse> postAsync(string endpoint, dynamic body)
        {
            var client = new RestClient(this.apiUrl);
            var request = new RestRequest(endpoint, Method.POST);

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
    }
}