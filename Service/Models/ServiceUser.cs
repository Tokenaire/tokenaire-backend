using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Tokenaire.Database.Models;
using Tokenaire.Service.Enums;

namespace Tokenaire.Service.Models
{
    public class ServiceUser
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public string ICOBTCAddress { get; set; }
        public string RegisteredFromReferralLinkId { get; internal set; }
    }
}