using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Tokenaire.Service.Enums;

namespace Tokenaire.Service.Models
{
    public class ServiceIcoFundsMyDetailsResult
    {
        public string ICOBTCAddress { get; set; }
        public string ReferralLinkUrl { get; set; }

        public long ReferralLinkRaisedBtc { get; set; }
        public long ReferralLinkEligibleBtc { get; set; }

        public long ICOBTCInvested { get; set; }
    }
}