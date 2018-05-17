using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using DnsClient;
using Loggly;
using Loggly.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Tokenaire.Database;
using Tokenaire.Database.Models;
using Tokenaire.Service.Models;
using Tokenaire.Service.Models.Models;

namespace Tokenaire.Service
{
    public interface ILogService
    {
        Task<bool> Error(ServiceLogError error);
    }

    public class LogService : ILogService
    {
        private readonly ILogglyClient logglyClient;

        public LogService(IConfiguration configuration, IHostingEnvironment env, ILogglyClient logglyClient, ISettingsService settingsService)
        {
            var config = LogglyConfig.Instance;
            config.CustomerToken = settingsService.LogglyBackendToken;
            config.ApplicationName = $"Tokenaire-{env.EnvironmentName}";

            config.Transport.EndpointHostname = "logs-01.loggly.com";
            config.Transport.EndpointPort = 443;
            config.Transport.LogTransport = LogTransport.Https;

            this.logglyClient = logglyClient;
        }

        public async Task<bool> Error(ServiceLogError error)
        {
            var logEvent = new LogglyEvent();

            logEvent.Data.Add("message", new
            {
                Date = DateTime.UtcNow,
                error = error.Message
            });

            await this.logglyClient.Log(logEvent);
            return true;
        }
    }
}

