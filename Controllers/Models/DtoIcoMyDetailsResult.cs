using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Controllers.Models
{
    public class DtoIcoMyDetailsResult
    {
        public string ICOBTCAddress { get; set; }
        public string ICOBTCRefundAddress { get; set; }

        public string ICOBTCInvested { get; set; }

        public string ReferralLinkUrl { get; set; }
        public string ReferralLinkEligibleBtc { get; set; }
        public string ReferralLinkRaisedBtc { get; set; }

        public long AireToReceive { get; set; }
        public bool IsKyced { get; set; }

        public int ReferralLinkUsedByPeople { get; set; }

        public double DiscountRate { get; internal set; }
        public double OneAireInSatoshies { get; set; }
    }
}