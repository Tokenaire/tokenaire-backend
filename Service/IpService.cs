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
        string GetClientIpXForward();
        string GetClientIpDirect();

        string GetClientIp(bool tryUseXForwardHeader, HttpContext httpContext);
    }

    public class IpService : IIpService
    {
        private readonly IHttpContextAccessor accessor;

        public IpService(IHttpContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        public string GetClientIpXForward()
        {
            return this.GetClientIp(true, this.accessor.HttpContext);
        }

        public string GetClientIpDirect()
        {
            return this.GetClientIp(false, this.accessor.HttpContext);
        }

        public string GetClientIp(bool tryUseXForwardHeader, HttpContext httpContext)
        {
            string ip = null;

            // todo support new "Forwarded" header (2014) https://en.wikipedia.org/wiki/X-Forwarded-For

            // X-Forwarded-For (csv list):  Using the First entry in the list seems to work
            // for 99% of cases however it has been suggested that a better (although tedious)
            // approach might be to read each IP from right to left and use the first public IP.
            // http://stackoverflow.com/a/43554000/538763
            //
            if (tryUseXForwardHeader)
                ip = GetHeaderValueAs<string>("X-Forwarded-For").SplitCsv().FirstOrDefault();

            // RemoteIpAddress is always null in DNX RC1 Update1 (bug).
            if (ip.IsNullOrWhitespace() && httpContext?.Connection?.RemoteIpAddress != null)
                ip = httpContext.Connection.RemoteIpAddress.ToString();

            if (ip.IsNullOrWhitespace())
                throw new Exception("Unable to determine caller's IP.");

            return ip;
        }

        private T GetHeaderValueAs<T>(string headerName)
        {
            StringValues values;

            if (accessor.HttpContext?.Request?.Headers?.TryGetValue(headerName, out values) ?? false)
            {
                string rawValues = values.ToString();

                if (!rawValues.IsNullOrWhitespace())
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
            }
            return default(T);
        }
    }

    public class CustomRemoteIpRateLimitParser : RemoteIpParser
    {
        private readonly IIpService ipService;

        public CustomRemoteIpRateLimitParser(IIpService ipService)
        {
            this.ipService = ipService;
        }

        public override IPAddress GetClientIp(HttpContext httpContext)
        {
            return IPAddress.Parse(ipService.GetClientIp(true, httpContext));
        }
    }
}

