using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Tokenaire.Service.Enums;

namespace Tokenaire.Service.Models
{
    public enum ServiceUserReferralLinkType {
        ICO = 1
    }

    public class ServiceUserReferralLinkCreate
    {
        public string UserId { get; set; }
        public ServiceUserReferralLinkType Type { get; set; }
    }
}