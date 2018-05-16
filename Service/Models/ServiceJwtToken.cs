using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tokenaire.Service.Models
{
    public class ServiceJwtToken
    {
        public string Id { get; set; }
        public string AuthToken { get; set; }

        public int ExpiresIn { get; set; }
    }
}