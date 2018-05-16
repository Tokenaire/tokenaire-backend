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
using Tokenaire.Database;
using Tokenaire.Database.Models;
using Tokenaire.Service.Models;
using Tokenaire.Service.Models.Models;

namespace Tokenaire.Service
{
    public interface IJwtService
    {
        Task<ClaimsIdentity> GenerateClaimsIdentity(string userName, string id);
        Task<ServiceJwtToken> GenerateJwt(ClaimsIdentity identity, string userName);
    }

    public class JwtService : IJwtService
    {
        private readonly ServiceJwtIssuerOptions _jwtOptions;

        public JwtService(IOptions<ServiceJwtIssuerOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
            ThrowIfInvalidOptions(_jwtOptions);
        }

        public async Task<ServiceJwtToken> GenerateJwt(ClaimsIdentity identity, string userName)
        {
            var serializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented };

            return new ServiceJwtToken()
            {
                Id = identity.Claims.Single(c => c.Type == "id").Value,
                AuthToken = await this.GenerateEncodedToken(userName, identity),
                ExpiresIn = (int)this._jwtOptions.ValidFor.TotalSeconds
            };
        }

        private async Task<string> GenerateEncodedToken(string userName, ClaimsIdentity identity)
        {
            var claims = new[]
         {
                 new Claim(JwtRegisteredClaimNames.Sub, userName),
                 new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
                 new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
                 identity.FindFirst(ServiceConstants.Strings.JwtClaimIdentifiers.Rol),
                 identity.FindFirst(ServiceConstants.Strings.JwtClaimIdentifiers.Id)
             };

            // Create the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: _jwtOptions.NotBefore,
                expires: _jwtOptions.Expiration,
                signingCredentials: _jwtOptions.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        public async Task<ClaimsIdentity> GenerateClaimsIdentity(string userName, string id)
        {
            return new ClaimsIdentity(new GenericIdentity(userName, "Token"), new[]
            {
                new Claim(ServiceConstants.Strings.JwtClaimIdentifiers.Id, id),
                new Claim(ServiceConstants.Strings.JwtClaimIdentifiers.Rol, ServiceConstants.Strings.JwtClaims.ApiAccess)
            });
        }

        /// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
        private static long ToUnixEpochDate(DateTime date)
          => (long)Math.Round((date.ToUniversalTime() -
                               new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                              .TotalSeconds);

        private static void ThrowIfInvalidOptions(ServiceJwtIssuerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ValidFor <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(ServiceJwtIssuerOptions.ValidFor));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(ServiceJwtIssuerOptions.SigningCredentials));
            }

            if (options.JtiGenerator == null)
            {
                throw new ArgumentNullException(nameof(ServiceJwtIssuerOptions.JtiGenerator));
            }
        }
    }
}

