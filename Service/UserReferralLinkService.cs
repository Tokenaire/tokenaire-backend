using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DnsClient;
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
    public interface IUserReferralLinkService
    {
        Task<ServiceUserReferralLinkCreateResult> CreateAsync(ServiceUserReferralLinkCreate model);
        Task<bool> IsValidReferralLinkAsync(string referralLinkId);
        Task<string> GetReferralLinkIdAsync(string userId, ServiceUserReferralLinkType type);
    }

    public class UserReferralLinkService : IUserReferralLinkService
    {
        private readonly UserManager<DatabaseUser> userManager;
        private readonly TokenaireContext tokenaireContext;

        public UserReferralLinkService(UserManager<DatabaseUser> userManager,
            TokenaireContext tokenaireContext)
        {
            this.userManager = userManager;
            this.tokenaireContext = tokenaireContext;
        }

        public async Task<ServiceUserReferralLinkCreateResult> CreateAsync(ServiceUserReferralLinkCreate model)
        {
            var referralLink = new DatabaseUserReferralLink()
            {
                UserId = model.UserId,
                Type = this.GetDbType(model.Type)
            };

            await this.tokenaireContext.UserReferralLinks.AddAsync(referralLink);
            await this.tokenaireContext.SaveChangesAsync();

            return new ServiceUserReferralLinkCreateResult()
            {
                UserReferralLinkId = referralLink.Id
            };
        }

        public async Task<string> GetReferralLinkIdAsync(string userId, ServiceUserReferralLinkType serviceType)
        {
            var type = this.GetDbType(serviceType);
            var result = await this.tokenaireContext
                .UserReferralLinks
                .SingleAsync(x => 
                    x.UserId == userId &&
                    x.Type == type);

            return result.Id;
        }

        public async Task<bool> IsValidReferralLinkAsync(string referralLinkId)
        {
            if (referralLinkId == null)
            {
                return true;
            }

            return await this.tokenaireContext
                .UserReferralLinks
                .FirstOrDefaultAsync(x => x.Id == referralLinkId) != null;
        }

        private DatabaseUserReferralLinkType GetDbType(ServiceUserReferralLinkType serviceReferralLinkType)
        {
            DatabaseUserReferralLinkType? type = null;
            if (serviceReferralLinkType == ServiceUserReferralLinkType.ICO)
            {
                type = DatabaseUserReferralLinkType.ICO;
            }

            if (type == null)
            {
                throw new InvalidOperationException("No type found for serviceuserefferalLInkType" + serviceReferralLinkType);
            }

            return type.Value;
        }
    }
}

