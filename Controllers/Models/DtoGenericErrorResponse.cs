using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Controllers.Models
{
    public class DtoGenericErrorResponse
    {
        public List<DtoGenericError> Errors { get; set; }
    }
}