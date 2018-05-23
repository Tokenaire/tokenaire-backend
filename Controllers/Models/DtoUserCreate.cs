using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Controllers.Models
{
    public class DtoUserCreate
    {
        public string Email { get; set; }
        public string HashedPassword { get; set; }

        public string EncryptedSeed { get; set; }

        public string Address { get; set; }
        public string PublicKey { get; set; }
        public string Signature { get; set; }

        public string RegisteredFromReferralLinkId { get; set; }
    }
}