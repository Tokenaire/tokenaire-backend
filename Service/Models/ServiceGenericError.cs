using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Tokenaire.Service.Enums;

namespace Tokenaire.Service.Models
{
    public class ServiceGenericError
    {
        public ServiceGenericErrorEnum Code { get; set; }

        public string Message { get; set; }
    }
}