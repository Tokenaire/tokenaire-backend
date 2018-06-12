using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
using tokenaire_backend.Extensions;
using WavesCS;

namespace Tokenaire.Service
{
    public interface IChangellyService
    {
    }

    public class ChangellyService : IChangellyService
    {
        private readonly Encoding U8 = Encoding.UTF8;
        private readonly string apiKey;
        private readonly string apiSecret;
        private readonly string apiUrl;

        public ChangellyService(IConfiguration configuration, ISettingsService settingsService)
        {
            apiKey = settingsService.ChangellyApiKey;
            apiSecret = settingsService.ChangellyApiSecret;
            apiUrl = configuration.GetValue<string>("ChangellyApiUrl");
        }

        private async Task<RestResponse> postAsync(ChangellyApiInput obj)
        {
            var client = new RestClient(this.apiUrl);
            var request = new RestRequest("/", Method.POST);

            string message = obj.Serialize();

            HMACSHA512 hmac = new HMACSHA512(U8.GetBytes(apiSecret));
            byte[] hashmessage = hmac.ComputeHash(U8.GetBytes(message));
            string sign = hashmessage.ToHexString();

            request.AddHeader("api-key", apiKey);
            request.AddHeader("sign", sign);

            TaskCompletionSource<IRestResponse> taskCompletion = new TaskCompletionSource<IRestResponse>();
            RestRequestAsyncHandle handle = client.ExecuteAsync(request, r => taskCompletion.SetResult(r));
            return (RestResponse)(await taskCompletion.Task);
        }

        public class ChangellyApiInput
        {

            public string Id { get; set; }
            public string JsonRpc
            {
                get
                {
                    return "2.0";
                }
            }

            public string Method { get; set; }
            public ExpandoObject Params { get; set; }

            public string Serialize()
            {
                return JsonConvert.SerializeObject(this);
            }
        }
    }
}

