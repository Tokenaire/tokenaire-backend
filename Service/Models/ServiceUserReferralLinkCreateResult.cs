using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Tokenaire.Service.Enums;

namespace Tokenaire.Service.Models
{
    public class ServiceUserReferralLinkCreateResult
    {
        public string UserReferralLinkId { get; set; }
    }
}