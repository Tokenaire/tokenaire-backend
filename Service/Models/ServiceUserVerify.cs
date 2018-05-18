using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Tokenaire.Service.Enums;

namespace Tokenaire.Service.Models
{
    public class ServiceUserVerify
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}