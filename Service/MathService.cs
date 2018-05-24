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
using Microsoft.Extensions.Caching.Memory;
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
    public interface IMathService
    {
        double ConvertSatoshiesToBTC(long satoshies);
        string ConvertSatoshiesToBTCFormatted(long satoshies);
    }

    public class MathService : IMathService
    {
        private readonly double oneBTCInSatoshies = 100000000;

        public double ConvertSatoshiesToBTC(long satoshies) {
            return Math.Round(satoshies / this.oneBTCInSatoshies, 8);
        }

        public string ConvertSatoshiesToBTCFormatted(long satoshies) {
            var value = satoshies / this.oneBTCInSatoshies;
            var formatted = String.Format("{0:0.00000000}", value);
            return formatted;
        } 
    }
}

