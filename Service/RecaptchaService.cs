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
    public interface IRecaptchaService
    {
        Task<bool> IsValidCaptchaResponse(string captchaResponse, string ip);
    }

    public class RecaptchaService : IRecaptchaService
    {
        private readonly string apiUrl;
        private readonly ISettingsService settingsService;

        public RecaptchaService(IConfiguration configuration, ISettingsService settingsService)
        {
            apiUrl = configuration.GetValue<string>("RecaptchaApiUrl");
            this.settingsService = settingsService;
        }

        public async Task<bool> IsValidCaptchaResponse(string captchaResponse, string ip) {
            var response = await postAsync("/", new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("secret", settingsService.ReCaptchaKey),
                new KeyValuePair<string, string>("response", captchaResponse),
                new KeyValuePair<string, string>("remoteip", ip)
            }, new {});

            if (response.StatusCode == HttpStatusCode.OK) {
                dynamic result = JsonConvert.DeserializeObject(response.Content);
                return result.success == true;
            }

            return false;
        }

        private async Task<RestResponse> postAsync(string endpoint,  List<KeyValuePair<string, string>> queryParameters, dynamic body)
        {
            var client = new RestClient(this.apiUrl);
            var request = new RestRequest(endpoint, Method.POST);

            foreach (var pair in queryParameters)
            {
                request.AddQueryParameter(pair.Key, pair.Value);
            }

            request.AddJsonBody(body);

            TaskCompletionSource<IRestResponse> taskCompletion = new TaskCompletionSource<IRestResponse>();
            RestRequestAsyncHandle handle = client.ExecuteAsync(request, r => taskCompletion.SetResult(r));
            return (RestResponse)(await taskCompletion.Task);
        }
    }
}

