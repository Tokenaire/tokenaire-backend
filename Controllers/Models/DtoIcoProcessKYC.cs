using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Controllers.Models
{
    public class DtoIcoProcessKYCReview
    {
        public string ModerationComment { get; set; }
        public string ClientComment { get; set; }
        public string ReviewAnswer { get; set; }
        public List<string> RejectLabels { get; set; }
        public string ReviewRejectType { get; set; }
    }

    public class DtoIcoProcessKYC
    {
        public string ApplicantId { get; set; }
        public string InspectionId { get; set; }
        public bool Success { get; set; }
        public string CorrelationId { get; set; }
        public string ExternalUserId { get; set; }
        public string Details { get; set; }
        public string Type { get; set; }

        public DtoIcoProcessKYCReview Review { get; set; }
    }
}