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
using Microsoft.AspNetCore.Http;
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
    public interface IIpService
    {
        Task<string> GetClientIp();
    }

    public class IpService : IIpService
    {
        private readonly IHttpContextAccessor accessor;

        public IpService(IHttpContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        public async Task<string> GetClientIp()
        {
            return this.accessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }
    }
}

