using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Tokenaire.Service.Enums;

namespace Tokenaire.Service.Models
{
    public class ServiceIcoFundsMyDetailsResult
    {
        public string ICOBTCAddress { get; set; }
        public string ICOBTCRefundAddress { get; set; }


        public string ReferralLinkUrl { get; set; }
        public long ReferralLinkRaisedBtcSatoshies { get; set; }
        public long ReferralLinkEligibleBtcSatoshies { get; set; }
        public long ICOBTCInvestedSatoshies { get; set; }


        public int ReferralLinkRate { get; set; }
        public long OneAireInSatoshies { get; set; }
        public long AIREToReceive { get; set; }
    }
}