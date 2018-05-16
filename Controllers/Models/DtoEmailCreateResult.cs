using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Controllers.Models
{
    public class DtoEmailCreateResult
    {
        public string Succeeded { get; set; }
    }
}