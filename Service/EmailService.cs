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
    public interface IEmailService
    {
        Task<ServiceEmailCreateResult> Create(ServiceEmailCreate model);

        Task<bool> IsValid(string email);
    }

    public class EmailService : IEmailService
    {
        private readonly TokenaireContext tokenaireContext;
        private readonly ILookupClient lookupClient;

        public EmailService(TokenaireContext tokenaireContext, ILookupClient lookupClient)
        {
            this.tokenaireContext = tokenaireContext;
            this.lookupClient = lookupClient;
        }

        public async Task<ServiceEmailCreateResult> Create(ServiceEmailCreate model)
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

                return new ServiceEmailCreateResult()
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

                return new ServiceEmailCreateResult()
                {
                    Errors = errors
                };
            }

// step3; email has to be actually valid
// and dns records should exist!
            if (!await this.IsValid(email)) {
                errors.Add(new ServiceGenericError()
                {
                    Code = ServiceGenericErrorEnum.BadEmail,
                    Message = "Email is invalid"
                });

                return new ServiceEmailCreateResult()
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

            return new ServiceEmailCreateResult()
            {
                Errors = errors
            };
        }

        public async Task<bool> IsValid(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                var domain = mailAddress.Host;

                var result = await this.lookupClient.QueryAsync(domain, QueryType.ANY).ConfigureAwait(false);
                var records = result.Answers.Where(record => record.RecordType == DnsClient.Protocol.ResourceRecordType.A ||
                                                             record.RecordType == DnsClient.Protocol.ResourceRecordType.AAAA ||
                                                             record.RecordType == DnsClient.Protocol.ResourceRecordType.MX);
                return records.Any();
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}

