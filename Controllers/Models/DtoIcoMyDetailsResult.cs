using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Controllers.Models
{
    public class DtoIcoMyDetailsResult
    {
        public string ICOBTCAddress { get; set; }
        public long ICOBTCInvested { get;  set; }


        public string ReferralLinkUrl { get; set; }
        public long ReferralLinkEligibleBtc { get; set; }
        public long ReferralLinkRaisedBtc { get; set; }
        public long? AireBalance { get; set; }
    }
}