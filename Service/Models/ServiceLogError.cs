using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tokenaire.Service.Models
{
    public class ServiceLogError
    {
        public string Message { get; set; }
    }
}