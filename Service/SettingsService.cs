using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using org.whispersystems.curve25519;
using Tokenaire.Database;
using Tokenaire.Database.Models;
using Tokenaire.Service.Enums;
using Tokenaire.Service.Models;
using WavesCS;

namespace Tokenaire.Service
{
    public interface ISettingsService
    {
        string JwtSecret { get; }

        string SumSubApiUrl { get; }
        string SumSubApiKey { get; }

        string ChangellyApiKey { get; }
        string ChangellyApiSecret { get; }

        string BitGoICOFundsApiKey { get; }
        string BitGoICORefundsWalletPassword { get; }

        string WavesNodeApiKey { get; }
        string SendGridApiKey { get; }
        string LogglyBackendToken { get; }

        string WavesICOAireWalletPrivateKey { get; }

        string BackendPassword { get; }
    }

    public class SettingsService : ISettingsService
    {
        private readonly IConfiguration configuration;
        private readonly IHostingEnvironment environment;

        public string JwtSecret
        {
            get => this.EnvironmentVariable("JwtSecret", "TOKENAIRE_JWT_SECRET");
        }

        public string SumSubApiUrl
        {
            get => this.EnvironmentVariable("SumSubApiUrl", "TOKENAIRE_SUMSUB_API_URL");
        }

        public string SumSubApiKey
        {
            get => this.EnvironmentVariable("SumSubApiKey", "TOKENAIRE_SUMSUB_API_KEY");
        }

        public string ChangellyApiKey
        {
            get => this.EnvironmentVariable("ChangellyApiKey", "TOKENAIRE_CHANGELLY_API_KEY");
        }

        public string ChangellyApiSecret
        {
            get => this.EnvironmentVariable("ChangellyApiSecret", "TOKENAIRE_CHANGELLY_API_SECRET");
        }

        public string SendGridApiKey
        {
            get => this.EnvironmentVariable("SendGridApiKey", "TOKENAIRE_SENDGRID_API_KEY");
        }

        public string BitGoICOFundsApiKey
        {
            get => this.EnvironmentVariable("BitGoICOFundsApiKey", "TOKENAIRE_BITGO_ICOFUNDSAPIKEY");
        }

        public string BitGoICORefundsWalletPassword
        {
            get => this.EnvironmentVariable("BitGoICORefundsWalletPassword", "TOKENAIRE_BITGO_ICOREFUNDSWALLETPASSWORD");
        }

        public string WavesNodeApiKey
        {
            get => this.EnvironmentVariable("WavesNodeApiKey", "TOKENAIRE_WAVESNODE_API_KEY");
        }

        public string LogglyBackendToken
        {
            get => this.EnvironmentVariable("LogglyBackendToken", "TOKENAIRE_LOGGLY_BACKEND_TOKEN");
        }

        public string WavesICOAireWalletPrivateKey
        {
            get => this.EnvironmentVariable("WavesICOAireWalletPrivateKey", "TOKENAIRE_WAVESICOAIREWALLET_PRIVATEKEY");
        }

        public string BackendPassword
        {
            get => this.EnvironmentVariable("BackendPassword", "TOKENAIRE_BACKEND_PASSWORD");
        }

        public SettingsService(
            IConfiguration configuration,
            IHostingEnvironment environment
        )
        {
            this.configuration = configuration;
            this.environment = environment;
        }

        private string EnvironmentVariable(string key, string variable)
        {
            return this.NotNullCheck(key, Environment.GetEnvironmentVariable($"APPSETTING_{variable}"));
        }

        private string NotNullCheck(string key, string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new InvalidOperationException($"${key} is not allowed to be empty!");
            }

            return input;
        }
    }
}

