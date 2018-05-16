using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tokenaire.Service.Models
{
    public static class ServiceConstants
    {
        public static class Strings
        {
            public static class JwtClaimIdentifiers
            {
                public const string Rol = "rol", Id = "id";
            }

            public static class JwtClaims
            {
                public const string ApiAccess = "api_access";
            }
        }
    }
}