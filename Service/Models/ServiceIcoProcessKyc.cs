using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Tokenaire.Database.Models;
using Tokenaire.Service.Enums;

namespace Tokenaire.Service.Models
{
    public class ServiceIcoProcessKycReview
    {
        public string ModerationComment { get; set; }
        public string ClientComment { get; set; }
        public string ReviewAnswer { get; set; }
        public List<string> RejectLabels { get; set; }
        public string ReviewRejectType { get; set; }
    }

    public class ServiceIcoProcessKyc
    {
        public string ApplicantId { get; set; }
        public string InspectionId { get; set; }
        public bool IsSuccess { get; set; }
        public string CorrelationId { get; set; }
        public string ExternalUserId { get; set; }
        public string Details { get; set; }
        public string Type { get; set; }

        public ServiceIcoProcessKycReview Review { get; set; }
    }
}