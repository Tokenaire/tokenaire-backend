using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tokenaire.Service.Models
{
    public class ServiceEmailCreateSubscription
    {
        public string Value { get; set; }
        public string Ip { get; set; }
    }
}