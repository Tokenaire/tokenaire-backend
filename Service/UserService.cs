using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using org.whispersystems.curve25519;
using QRCoder;
using Tokenaire.Database;
using Tokenaire.Database.Models;
using Tokenaire.Service.Enums;
using Tokenaire.Service.Models;
using WavesCS;

namespace Tokenaire.Service
{
    public interface IUserService
    {
        Task<ServiceAuthenticatorKeyResult> GetAuthenticatorKeyAsync(string userId);
        Task<ServiceUser> GetUser(string userId);
        Task<List<ServiceUser>> GetUsers();

        Task<ServiceUserCreateResult> CreateAsync(ServiceUserCreate model);
        Task<ServiceUserLoginResult> LoginAsync(ServiceUserLogin model);

        Task<bool> IsTwoFactorAuthEnabled(string userId);

        Task<bool> IsEmailTakenAsync(string email);
        Task<bool> SendEmailConfirmationAsync(string email);
        Task<bool> VerifyAsync(ServiceUserVerify serviceUserVerify);

        Task<bool> SetTwoFactorAuth(string userId, string verificationCode, bool enabled);
    }

    public class UserService : IUserService
    {
        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        private readonly UserManager<DatabaseUser> userManager;
        private readonly UrlEncoder urlEncoder;
        private readonly IEmailService emailService;
        private readonly IJwtService jwtService;
        private readonly IConfiguration configuration;
        private readonly ICurve25519Service curve25519Service;
        private readonly IWavesAddressesNodeService wavesAddressesNodeService;
        private readonly IUserReferralLinkService userReferralLinkService;
        private readonly TokenaireContext tokenaireContext;

        public UserService(UserManager<DatabaseUser> userManager,
            UrlEncoder urlEncoder,
            IEmailService emailService,
            IJwtService jwtService,
            IConfiguration configuration,
            ICurve25519Service curve25519Service,
            IWavesAddressesNodeService wavesAddressesNodeService,
            IUserReferralLinkService userReferralLinkService,
            TokenaireContext tokenaireContext)
        {
            this.userManager = userManager;
            this.urlEncoder = urlEncoder;
            this.emailService = emailService;
            this.jwtService = jwtService;
            this.configuration = configuration;
            this.curve25519Service = curve25519Service;
            this.wavesAddressesNodeService = wavesAddressesNodeService;
            this.userReferralLinkService = userReferralLinkService;
            this.tokenaireContext = tokenaireContext;
        }

        public async Task<bool> IsTwoFactorAuthEnabled(string userId)
        {
            var user = await this.userManager.FindByIdAsync(userId);
            return await this.userManager.GetTwoFactorEnabledAsync(user);
        }

        public async Task<bool> SetTwoFactorAuth(string userId, string verificationCode, bool enabled)
        {
            var user = await this.userManager.FindByIdAsync(userId);
            var normalizedVerificationCode = verificationCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            var tokenProvider = this.userManager.Options.Tokens.AuthenticatorTokenProvider;

            if (!await this.userManager.VerifyTwoFactorTokenAsync(user, tokenProvider, normalizedVerificationCode))
            {
                return false;
            }

            var result = await this.userManager.SetTwoFactorEnabledAsync(user, enabled);
            return result.Succeeded;
        }

        public async Task<ServiceAuthenticatorKeyResult> GetAuthenticatorKeyAsync(string userId)
        {
            var user = await this.userManager.FindByIdAsync(userId);
            var unformattedKey = await this.userManager.GetAuthenticatorKeyAsync(user);

            if (string.IsNullOrEmpty(unformattedKey))
            {
                await this.userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await this.userManager.GetAuthenticatorKeyAsync(user);
            }

            var authenticatorUri = GenerateQrCodeUri(user.Email, unformattedKey);

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(authenticatorUri, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new Base64QRCode(qrCodeData);
            var qrCodeImageAsBase64 = qrCode.GetGraphic(20);

            return new ServiceAuthenticatorKeyResult()
            {
                Key = FormatKey(unformattedKey),
                KeyAsImage = qrCodeImageAsBase64
            };
        }

        public async Task<List<ServiceUser>> GetUsers()
        {
            var dbUsers = await this.userManager.Users
                .Include(u => u.RegistrationInfo)
                .ToArrayAsync();

            return dbUsers.Select((dbUser) =>
            {
                return new ServiceUser()
                {
                    Id = dbUser.Id,
                    Address = dbUser.Address,
                    ICOKyced = dbUser.ICOKyced,
                    ICOBTCAddress = dbUser.ICOBTCAddress,
                    ICOBTCRefundAddress = dbUser.ICOBTCRefundAddress,
                    RegisteredFromReferralLinkId = dbUser.RegistrationInfo.RegisteredFromReferralLinkId
                };
            }).ToList();
        }

        public async Task<ServiceUser> GetUser(string userId)
        {
            var dbUser = await this.userManager.Users
                .Include(u => u.RegistrationInfo)
                .FirstAsync(x => x.Id == userId);

            return new ServiceUser()
            {
                Id = dbUser.Id,
                Address = dbUser.Address,
                ICOKyced = dbUser.ICOKyced,
                ICOBTCAddress = dbUser.ICOBTCAddress,
                ICOBTCRefundAddress = dbUser.ICOBTCRefundAddress,
                RegisteredFromReferralLinkId = dbUser.RegistrationInfo.RegisteredFromReferralLinkId
            };
        }

        public async Task<ServiceUserCreateResult> CreateAsync(ServiceUserCreate model)
        {
            var modelErrors = model.GetErrors();
            if (modelErrors.Count > 0)
            {
                return new ServiceUserCreateResult()
                {
                    Errors = modelErrors
                };
            }

            var isValidProof = await this.curve25519Service.VerifySignature(
                Base58.Decode(model.PublicKey),
                Base58.Decode("tokenaireverification"),
                Base58.Decode(model.Signature));

            // we have to validate the public key & address and signature,
            // before we will let the user through.
            if (!isValidProof)
            {
                return new ServiceUserCreateResult()
                {
                    Errors = new List<ServiceGenericError>() {
                        new ServiceGenericError()
                        {
                            Code = ServiceGenericErrorEnum.InvalidSignature,
                            Message = "Invalid signature"
                        }
                    }
                };
            }

            if (await this.wavesAddressesNodeService.GetAddressFromPublicKey(model.PublicKey) != model.Address)
            {
                return new ServiceUserCreateResult()
                {
                    Errors = new List<ServiceGenericError>() {
                        new ServiceGenericError()
                        {
                            Code = ServiceGenericErrorEnum.InvalidAddress,
                            Message = "Invalid address"
                        }
                    }
                };
            }

            if (!await this.emailService.IsValid(model.Email))
            {
                return new ServiceUserCreateResult()
                {
                    Errors = new List<ServiceGenericError>() {
                        new ServiceGenericError()
                        {
                            Code = ServiceGenericErrorEnum.BadEmail,
                            Message = "Bad email"
                        }
                    }
                };
            }

            var email = model.Email?.ToLowerInvariant();
            if (await this.userManager.FindByEmailAsync(email) != null)
            {
                return new ServiceUserCreateResult()
                {
                    Errors = new List<ServiceGenericError>() {
                        new ServiceGenericError()
                        {
                            Code = ServiceGenericErrorEnum.EmailNotUnique,
                            Message = "Email is not unique"
                        }
                    }
                };
            }

            var ipRegisteredLessThanFiveMinutesAgo = await this.tokenaireContext.Users.FirstOrDefaultAsync(u =>
                u.RegisteredFromIP == model.RegisteredFromIP &&
                TokenaireContext.DateDiff("minute", u.RegisteredDate, DateTime.UtcNow) <= 5);

            if (ipRegisteredLessThanFiveMinutesAgo != null)
            {
                return new ServiceUserCreateResult()
                {
                    Errors = new List<ServiceGenericError>() {
                        new ServiceGenericError()
                        {
                            Code = ServiceGenericErrorEnum.IpAlreadyRegisteredWait,
                            Message = "IP registered already less than five minutes ago, please wait."
                        }
                    }
                };
            }

            var userId = Guid.NewGuid().ToString();
            var userRegistrationInfo = new DatabaseUserRegistrationInfo()
            {
                UserId = userId
            };

            if (await this.userReferralLinkService.IsValidReferralLinkAsync(model.RegisteredFromReferralLinkId))
            {
                userRegistrationInfo.RegisteredFromReferralLinkId = model.RegisteredFromReferralLinkId;
            }

            var user = new DatabaseUser()
            {
                Id = userId,

                UserName = email,
                Email = email,
                EncryptedSeed = model.EncryptedSeed,

                BTCAddress = model.BTCAddress,
                Address = model.Address,
                PublicKey = model.PublicKey,
                Signature = model.Signature,

                ICOBTCAddress = model.ICOBTCAddress,

                RegisteredFromIP = model.RegisteredFromIP,
                RegisteredDate = DateTime.UtcNow,

                RegistrationInfo = userRegistrationInfo,
                ReferralLinks = new List<DatabaseUserReferralLink>() {
                    new DatabaseUserReferralLink() {
                        UserId = userId,
                        Type = DatabaseUserReferralLinkType.ICO
                    }
                }
            };

            var result = await this.userManager.CreateAsync(user, model.HashedPassword);
            if (!result.Succeeded)
            {
                return new ServiceUserCreateResult()
                {
                    Errors = new List<ServiceGenericError>() {
                        new ServiceGenericError()
                        {
                            Code = ServiceGenericErrorEnum.Unknown,
                            Message = "Generic user creation errors"
                        }
                    }
                };
            }

            await this.SendEmailConfirmationAsync(email);
            return new ServiceUserCreateResult()
            {
                Errors = new List<ServiceGenericError>(),
            };
        }

        public async Task<bool> IsEmailTakenAsync(string email)
        {
            return await this.userManager.FindByEmailAsync(email) != null;
        }

        public async Task<ServiceUserLoginResult> LoginAsync(ServiceUserLogin model)
        {
            var modelErrors = model.GetErrors();
            if (modelErrors.Count > 0)
            {
                return new ServiceUserLoginResult()
                {
                    Errors = modelErrors
                };
            }

            var email = model.Email?.ToLowerInvariant();
            var invalidEmailOrPasswordResult = new ServiceUserLoginResult()
            {
                Errors = new List<ServiceGenericError>() {
                        new ServiceGenericError()
                        {
                            Code = ServiceGenericErrorEnum.InvalidEmailOrPassword,
                            Message = "Invalid email or password"
                        }
                    }
            };

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(model.HashedPassword))
            {
                return invalidEmailOrPasswordResult;
            }

            var user = await this.userManager.FindByNameAsync(email);
            if (user == null)
            {
                return invalidEmailOrPasswordResult;
            }

            if (await this.userManager.IsLockedOutAsync(user))
            {
                return new ServiceUserLoginResult()
                {
                    Errors = new List<ServiceGenericError>() {
                        new ServiceGenericError()
                        {
                            Code = ServiceGenericErrorEnum.LockedOut,
                            Message = "Locked out"
                        }
                    }
                };
            }

            if (!await this.userManager.CheckPasswordAsync(user, model.HashedPassword))
            {
                await this.userManager.AccessFailedAsync(user);
                return invalidEmailOrPasswordResult;
            }

            await this.userManager.ResetAccessFailedCountAsync(user);

            var identity = await jwtService.GenerateClaimsIdentity(email, user.Id);
            var emailConfirmed = await this.userManager.IsEmailConfirmedAsync(user);
            var isFirstTimeLogging = user.LastLoginDate == null;
            if (!emailConfirmed)
            {
                return new ServiceUserLoginResult()
                {
                    Errors = new List<ServiceGenericError>() {
                        new ServiceGenericError()
                        {
                            Code = ServiceGenericErrorEnum.EmailNeedsConfirming,
                            Message = "Email needs confirming"
                        }
                    }
                };
            }

            if (user.TwoFactorEnabled)
            {
                var tokenProvider = this.userManager.Options.Tokens.AuthenticatorTokenProvider;
                var verificationCode = model.VerificationCode != null ? model.VerificationCode : "";
                var normalizedVerificationCode = verificationCode
                    .Replace(" ", string.Empty)
                    .Replace("-", string.Empty);

                if (string.IsNullOrEmpty(normalizedVerificationCode))
                {
                    return new ServiceUserLoginResult()
                    {
                        Errors = new List<ServiceGenericError>() {
                            new ServiceGenericError()
                            {
                                Code = ServiceGenericErrorEnum.TwoFactorRequired,
                                Message = "Two factor required"
                            }
                        }
                    };
                }

                if (!await this.userManager.VerifyTwoFactorTokenAsync(user, tokenProvider, normalizedVerificationCode))
                {
                    return new ServiceUserLoginResult()
                    {
                        Errors = new List<ServiceGenericError>() {
                            new ServiceGenericError()
                            {
                                Code = ServiceGenericErrorEnum.InvalidTwoFactor,
                                Message = "Invalid two factor"
                            }
                        }
                    };
                }
            }

            user.LastLoginDate = DateTime.UtcNow;
            await this.tokenaireContext.SaveChangesAsync();

            var jwt = await this.jwtService.GenerateJwt(identity, email);
            var isTwoFactorAuthEnabled = await this.IsTwoFactorAuthEnabled(user.Id);

            return new ServiceUserLoginResult()
            {
                Errors = new List<ServiceGenericError>(),
                EncryptedSeed = user.EncryptedSeed,
                Jwt = jwt,

                IsFirstTimeLogging = isFirstTimeLogging,
                IsTwoFactorAuthEnabled = isTwoFactorAuthEnabled
            };
        }

        public async Task<bool> SendEmailConfirmationAsync(string email)
        {
            // at this point,
            // user has been created successfully,
            // and there is not much left to do,
            // he however has to verify his email
            // before he can do any logging in.
            var user = await this.userManager.FindByEmailAsync(email);
            var emailVerificationLink = await this.GenerateEmailVerificationCodeUrlAsync(user);
            var emailSentSuccessfully = await this.emailService.SendSingleEmailUsingTemplate(new ServiceEmailSendUsingTemplate()
            {
                TemplateId = ServiceEmailTemplateEnum.UserEmailVerificationStep1,
                ToEmail = email,

                Substitutions = new List<ServiceEmailSendUsingTemplateSubstitution>(){
                    new ServiceEmailSendUsingTemplateSubstitution("VERIFICATION_LINK", emailVerificationLink)
                }
            });

            return emailSentSuccessfully;
        }


        public async Task<bool> VerifyAsync(ServiceUserVerify serviceUserVerify)
        {
            if (string.IsNullOrEmpty(serviceUserVerify.Email) || string.IsNullOrEmpty(serviceUserVerify.Code))
            {
                return false;
            }

            var decodedEmail = Encoding.UTF8.GetString(Base58.Decode(serviceUserVerify.Email));
            var decodedCode = Encoding.UTF8.GetString(Base58.Decode(serviceUserVerify.Code));

            var user = await this.userManager.FindByEmailAsync(decodedEmail);
            if (user == null)
            {
                return false;
            }

            if (await this.userManager.IsEmailConfirmedAsync(user))
            {
                return true;
            }

            var result = await this.userManager.ConfirmEmailAsync(user, decodedCode);
            return result.Succeeded;
        }

        private async Task<string> GenerateEmailVerificationCodeUrlAsync(DatabaseUser user)
        {
            var code = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = this.configuration.GetValue<string>("TokenairePlatformUrl");

            var encodedCode = Base58.Encode(Encoding.UTF8.GetBytes(code));
            var encodedEmail = Base58.Encode(Encoding.UTF8.GetBytes(user.Email));

            return $"{callbackUrl}/verify?code={encodedCode}&email={encodedEmail}";
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                AuthenticatorUriFormat,
                this.urlEncoder.Encode("Tokenaire"),
                this.urlEncoder.Encode(email),
                unformattedKey);
        }
    }
}

