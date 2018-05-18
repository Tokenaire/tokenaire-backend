using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tokenaire.Service.Enums
{
    public enum ServiceEmailTemplateEnum {
        UserEmailVerificationStep1 = 1,
        UserEmailRegistrationSucceededStep2 = 2
    }
}