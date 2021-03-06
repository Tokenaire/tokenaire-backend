using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tokenaire.Service.Enums
{
    public enum ServiceGenericErrorEnum {
        FieldEmpty = 1,
        EmailNotUnique = 2,
        BadEmail = 3,
        EmptyModel = 4,
        Unknown = 5,
        InvalidEmailOrPassword = 6,
        InvalidSignature = 7,
        InvalidAddress = 8,
        IpAlreadyRegisteredWait = 9,
        EmailNeedsConfirming = 10,
        LockedOut = 11,
        InvalidCaptcha = 12,
        TwoFactorRequired = 13,
        InvalidTwoFactor = 14 
    }
}