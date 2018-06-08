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
using Tokenaire.Service.Enums;
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
        private readonly IEmailService emailService;
        private readonly IConfiguration configuration;
        private readonly TokenaireContext tokenaireContext;

        public IcoKycService(
            IEmailService emailService,
            IConfiguration configuration,
            TokenaireContext tokenaireContext)
        {
            this.emailService = emailService;
            this.configuration = configuration;
            this.tokenaireContext = tokenaireContext;
        }

        public async Task<bool> IsKyced(string userId)
        {
            return await this.tokenaireContext
                .Users
                .FirstOrDefaultAsync(x => x.Id == userId && x.ICOKyced == true) != null;
        }

        public async Task<bool> ProcessKyc(ServiceIcoProcessKyc model)
        {

            // this should theoretically never happen,
            // however if you manually test it,
            // it might happen that externalUserId
            // doesn't mean anything!
            var user = await this.tokenaireContext.Users.FirstOrDefaultAsync(u => u.Id == model.ExternalUserId);
            var marketPlaceLink = this.configuration.GetValue<string>("TokenairePlatformUrl");

            if (user == null)
            {
                return false;
            }

            user.ICOKyced = model.IsSuccess;

            await this.tokenaireContext.ICOKyc.AddAsync(new DatabaseIcoKyc()
            {
                UserId = model.ExternalUserId,
                ApplicantId = model.ApplicantId,
                IsSuccessful = model.IsSuccess,
                Content = JsonConvert.SerializeObject(model),
                Date = DateTime.UtcNow
            });

            await this.tokenaireContext.SaveChangesAsync();

            if (model.IsSuccess)
            {
                await this.emailService.SendSingleEmailUsingTemplate(new ServiceEmailSendUsingTemplate()
                {
                    TemplateId = ServiceEmailTemplateEnum.UserKycSuccessful,
                    ToEmail = user.Email,

                    Substitutions = new List<ServiceEmailSendUsingTemplateSubstitution>() {
                        new ServiceEmailSendUsingTemplateSubstitution("MARKETPLACE_LINK", marketPlaceLink)
                    }
                });
            }
            else
            {
                await this.emailService.SendSingleEmailUsingTemplate(new ServiceEmailSendUsingTemplate()
                {
                    TemplateId = ServiceEmailTemplateEnum.UserKycFailed,
                    ToEmail = user.Email,

                    Substitutions = new List<ServiceEmailSendUsingTemplateSubstitution>() {
                        new ServiceEmailSendUsingTemplateSubstitution("MARKETPLACE_LINK", marketPlaceLink)
                     }
                });
            }

            return true;
        }
    }
}

