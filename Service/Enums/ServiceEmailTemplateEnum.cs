using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tokenaire.Service.Enums
{
    public enum ServiceEmailTemplateEnum {
        UserEmailVerificationStep1 = 1,
        UserKycSuccessful = 2,
        UserKycFailed = 3,

        AirePurchaseSuccessful = 4,
        AireSentSuccessful = 5
    }
}