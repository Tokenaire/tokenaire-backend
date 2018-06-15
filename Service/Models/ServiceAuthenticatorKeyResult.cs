using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tokenaire.Service.Models
{
    public class ServiceAuthenticatorKeyResult
    {
        public string Key { get; set; }
        public string KeyAsImage { get; set; }
    }
}