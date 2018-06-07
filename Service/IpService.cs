using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using DnsClient;
using Loggly;
using Loggly.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Tokenaire.Database;
using Tokenaire.Database.Models;
using Tokenaire.Service.Models;
using Tokenaire.Service.Models.Models;
using tokenaire_backend.Extensions;

namespace Tokenaire.Service
{
    public interface IIpService
    {
        string GetClientIp();
    }

    public class IpService : IIpService
    {
        private readonly IHttpContextAccessor accessor;

        public IpService(IHttpContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        public string GetClientIp()
        {
            return this.accessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }
    }
}

