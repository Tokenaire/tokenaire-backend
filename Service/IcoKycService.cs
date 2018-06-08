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
    public interface IIcoKycService
    {
        Task<bool> IsKyced(string userId);
        Task<bool> ProcessKyc(ServiceIcoProcessKyc model);
    }

    public class IcoKycService : IIcoKycService
    {
        private readonly TokenaireContext tokenaireContext;

        public IcoKycService(
            TokenaireContext tokenaireContext)
        {
            this.tokenaireContext = tokenaireContext;
        }

        public async Task<bool> IsKyced(string userId) {
            return await this.tokenaireContext
                .ICOKyc
                .FirstOrDefaultAsync(u => u.UserId == userId && u.IsSuccessful == true) != null;
        }

        public async Task<bool> ProcessKyc(ServiceIcoProcessKyc model) {
            await this.tokenaireContext.ICOKyc.AddAsync(new DatabaseIcoKyc() {
                UserId = model.ExternalUserId,
                ApplicantId = model.ApplicantId,
                IsSuccessful = model.IsSuccess,
                Content = JsonConvert.SerializeObject(model)
            });

            await this.tokenaireContext.SaveChangesAsync();
            return true;
        }
    }
}

