using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Controllers.Models
{
    public class DtoUserLogin
    {
        public string Email { get; set; }
        public string HashedPassword { get; set; }
    }
}