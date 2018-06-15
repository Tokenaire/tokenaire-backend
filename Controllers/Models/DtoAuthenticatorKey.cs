using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Controllers.Models
{
    public class DtoAuthenticatorKey
    {
        public string Key { get; set; }
        public string KeyAsImage { get; set; }
    }
}