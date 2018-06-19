using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid;
using SendGrid.Helpers.Mail;
using Tokenaire.Database;
using Tokenaire.Database.Models;
using Tokenaire.Service.Enums;
using Tokenaire.Service.Models;

namespace Tokenaire.Service
{
    public interface IEmailService
    {
        Task<bool> IsValid(string email);
        Task<bool> SendSingleEmailUsingTemplate(ServiceEmailSendUsingTemplate model);
    }

    public class EmailService : IEmailService
    {
        private readonly ILookupClient lookupClient;
        private readonly ISettingsService settingsService;
        private readonly IConfiguration configuration;

        private readonly Dictionary<ServiceEmailTemplateEnum, string> templateMap = new Dictionary<ServiceEmailTemplateEnum, string>(){
            {ServiceEmailTemplateEnum.UserEmailVerificationStep1, "0752cef0-f918-4728-89d6-7968ebebb6d8"},
            {ServiceEmailTemplateEnum.UserKycSuccessful, "96c30b22-f86e-4f54-a78d-29fdfddaf2ab"},
            {ServiceEmailTemplateEnum.UserKycFailed, "c498d6b6-b426-411e-ae94-0d80b499bcb3"}
        };

        public EmailService(
            ILookupClient lookupClient,
            ISettingsService settingsService,
            IConfiguration configuration)
        {
            this.lookupClient = lookupClient;
            this.settingsService = settingsService;
            this.configuration = configuration;
        }

        public async Task<bool> SendSingleEmailUsingTemplate(ServiceEmailSendUsingTemplate model)
        {
            var client = new SendGridClient(settingsService.SendGridApiKey);
            var fromEmailNormalized = model.FromEmail ?? "support@tokenaire.club";
            var fromNameNormalized = model.FromName ?? "Tokenaire";

            var msg = new SendGridMessage()
            {
                From = new EmailAddress(fromEmailNormalized, fromNameNormalized),
                TemplateId = this.templateMap[model.TemplateId],
            };

            if (model.Substitutions != null) {
                foreach (var sub in model.Substitutions) {
                    msg.AddSubstitution($"%{sub.Key}%", sub.Value);
                }
            }

            msg.AddTo(new EmailAddress(model.ToEmail));
            var response = await client.SendEmailAsync(msg);
            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> IsValid(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                var domain = mailAddress.Host;

                var result = await this.lookupClient.QueryAsync(domain, QueryType.MX).ConfigureAwait(false);
                var records = result.Answers.Where(record => record.RecordType == DnsClient.Protocol.ResourceRecordType.MX);

                return records.Any();
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}

