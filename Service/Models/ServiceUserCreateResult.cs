using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tokenaire.Service.Models
{
    public class ServiceUserCreateResult
    {
        public List<ServiceGenericError> Errors { get; set; }
    }
}