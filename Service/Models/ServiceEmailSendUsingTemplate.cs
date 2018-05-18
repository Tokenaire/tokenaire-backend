using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Tokenaire.Service.Enums;

namespace Tokenaire.Service.Models
{
    public class ServiceEmailSendUsingTemplateSubstitution {
        public string Key { get; set; }
        public string Value { get; set; }

        public ServiceEmailSendUsingTemplateSubstitution(){}
        public ServiceEmailSendUsingTemplateSubstitution(string key, string value) {
            this.Key = key;
            this.Value = value;
        }
    }


    public class ServiceEmailSendUsingTemplate
    {
        public ServiceEmailTemplateEnum TemplateId { get; set; }
        public string ToEmail { get; set; }

        public string FromEmail { get; set; }
        public string FromName { get; set; }

        public List<ServiceEmailSendUsingTemplateSubstitution> Substitutions { get; set; }

    }
}