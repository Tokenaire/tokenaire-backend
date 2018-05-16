using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Tokenaire.Service.Enums;

namespace Tokenaire.Service.Models
{
    public class ServiceUserLogin
    {
        public string Email { get; set; }
        public string HashedPassword { get; set; }

        public List<ServiceGenericError> GetErrors()
        {
            var errors = new List<ServiceGenericError>();
            var email = this.Email?.ToLowerInvariant();

            if (string.IsNullOrEmpty(email))
            {
                errors.Add(new ServiceGenericError()
                {
                    Code = ServiceGenericErrorEnum.FieldEmpty,
                    Message = "Email can't be empty"
                });
            }

            if (string.IsNullOrEmpty(HashedPassword))
            {
                errors.Add(new ServiceGenericError()
                {
                    Code = ServiceGenericErrorEnum.FieldEmpty,
                    Message = "Hashed password can't be empty"
                });
            }

            return errors;
        }
    }
}