using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Controllers.Models
{
    public class DtoUserLoginResult
    {
        public string AuthToken { get; set; }
        public string EncryptedSeed { get; set; }
        public bool IsFirstTimeLogging { get; set; }
        public bool IsTwoFactorAuthEnabled { get; set; }
    }
}