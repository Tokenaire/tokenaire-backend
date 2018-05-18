using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tokenaire.Database;
using Tokenaire.Database.Models;
using Tokenaire.Service.Enums;
using Tokenaire.Service.Models;

namespace Tokenaire.Service
{
    public interface IEmailSubscriptionService
    {
        Task<ServiceEmailSubscriptionCreateResult> Create(ServiceEmailCreateSubscription model);
    }

    public class EmailSubscriptionService : IEmailSubscriptionService
    {
        private readonly TokenaireContext tokenaireContext;
        private readonly IEmailService emailService;

        public EmailSubscriptionService(TokenaireContext tokenaireContext, IEmailService emailService)
        {
            this.tokenaireContext = tokenaireContext;
            this.emailService = emailService;
        }

        public async Task<ServiceEmailSubscriptionCreateResult> Create(ServiceEmailCreateSubscription model)
        {
            var errors = new List<ServiceGenericError>();
            var email = model?.Value?.ToLowerInvariant();

// step1; validate that email actually is not null or empty
            if (string.IsNullOrEmpty(email))
            {
                errors.Add(new ServiceGenericError()
                {
                    Code = ServiceGenericErrorEnum.FieldEmpty,
                    Message = "Email can't be empty"
                });

                return new ServiceEmailSubscriptionCreateResult()
                {
                    Errors = errors
                };
            }

// step2; check if the email already exists,
// if it does,
// no big deal :)
            var dbEmail = await this.tokenaireContext.Emails.FirstOrDefaultAsync(x => x.Value == email);
            if (dbEmail != null)
            {
                errors.Add(new ServiceGenericError()
                {
                    Code = ServiceGenericErrorEnum.EmailNotUnique,
                    Message = "Email is not unique"
                });

                return new ServiceEmailSubscriptionCreateResult()
                {
                    Errors = errors
                };
            }

// step3; email has to be actually valid
// and dns records should exist!
            if (!await this.emailService.IsValid(email)) {
                errors.Add(new ServiceGenericError()
                {
                    Code = ServiceGenericErrorEnum.BadEmail,
                    Message = "Email is invalid"
                });

                return new ServiceEmailSubscriptionCreateResult()
                {
                    Errors = errors
                };
            }

            this.tokenaireContext.Emails.Add(new DatabaseEmail()
            {
                Value = email,
                Ip = model.Ip,
                Date = DateTime.UtcNow
            });

            await this.tokenaireContext.SaveChangesAsync();

            return new ServiceEmailSubscriptionCreateResult()
            {
                Errors = errors
            };
        }
    }
}

