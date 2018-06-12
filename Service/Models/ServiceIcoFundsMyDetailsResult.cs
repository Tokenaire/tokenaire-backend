using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Tokenaire.Service.Enums;

namespace Tokenaire.Service.Models
{
    public class ServiceIcoFundsMyDetailsResult
    {
        public bool IsKyced { get; set; }


        public string ICOBTCAddress { get; set; }
        public string ICOBTCRefundAddress { get; set; }


        public string ReferralLinkUrl { get; set; }
        public int ReferralLinkUsedByPeople { get; set; }

        public long ReferralLinkRaisedBtcSatoshies { get; set; }
        public long ReferralLinkEligibleBtcSatoshies { get; set; }
        public long ICOBTCInvestedSatoshies { get; set; }


        public int ReferralLinkRate { get; set; }
        public double OneAireInSatoshies { get; set; }
        public double DiscountRate { get; internal set; }

        public long AIREToReceive { get; set; }
    }
}