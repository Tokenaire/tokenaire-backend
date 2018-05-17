using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Controllers.Models
{
    public class DtoUserCreateResult
    {
        public string AuthToken { get; set; }
        public string ICOBTCAddress { get; set; }
    }
}