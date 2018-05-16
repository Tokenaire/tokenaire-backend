using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Tokenaire.Service.Enums;

namespace Tokenaire.Service.Models
{
    public class ServiceUserCreate
    {
        public string Email { get; set; }
        public string HashedPassword { get; set; }

        public string EncryptedSeed { get; set; }

        public string Address { get; set; }
        public string PublicKey { get; set; }
        public string Signature { get; set; }

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

            if (string.IsNullOrEmpty(EncryptedSeed))
            {
                errors.Add(new ServiceGenericError()
                {
                    Code = ServiceGenericErrorEnum.FieldEmpty,
                    Message = "Encrypted seed can't be empty"
                });
            }

            if (string.IsNullOrEmpty(Address))
            {
                errors.Add(new ServiceGenericError()
                {
                    Code = ServiceGenericErrorEnum.FieldEmpty,
                    Message = " Address can't be empty"
                });
            }

            if (string.IsNullOrEmpty(PublicKey))
            {
                errors.Add(new ServiceGenericError()
                {
                    Code = ServiceGenericErrorEnum.FieldEmpty,
                    Message = " Public key can't be empty"
                });
            }

            if (string.IsNullOrEmpty(Signature))
            {
                errors.Add(new ServiceGenericError()
                {
                    Code = ServiceGenericErrorEnum.FieldEmpty,
                    Message = "Signature can't be empty"
                });
            }

            return errors;
        }
    }
}