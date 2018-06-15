using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Tokenaire.Service.Models
{
    public class ServiceUserLoginResult
    {
        public List<ServiceGenericError> Errors { get; set; }
        public ServiceJwtToken Jwt { get; set; }
        public string EncryptedSeed { get; set; }
        public bool IsFirstTimeLogging { get; internal set; }
        public bool IsTwoFactorAuthEnabled { get; set; }
    }
}