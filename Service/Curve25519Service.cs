using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using org.whispersystems.curve25519;
using Tokenaire.Database;
using Tokenaire.Database.Models;
using Tokenaire.Service.Models;
using Tokenaire.Service.Models.Models;
using WavesCS;

namespace Tokenaire.Service
{
    public interface ICurve25519Service
    {
        Task<bool> VerifySignature(byte[] publicKey, byte[] message, byte[] signature);
    }

    public class Curve25519Service : ICurve25519Service
    {
        public async Task<bool> VerifySignature(byte[] publicKey, byte[] message, byte[] signature)
        {
            Curve25519 curve = Curve25519.getInstance(Curve25519.BEST);
            var status = curve.verifySignature(publicKey, message, signature);
            return status;
        }
    }
}

