using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using org.whispersystems.curve25519;
using Tokenaire.Database;
using Tokenaire.Database.Models;
using Tokenaire.Service.Enums;
using Tokenaire.Service.Models;
using WavesCS;

namespace Tokenaire.Service
{
    public interface IUserService
    {
        Task<ServiceUser> GetUser(string userId);
        Task<List<ServiceUser>> GetUsers();

        Task<ServiceUserCreateResult> CreateAsync(ServiceUserCreate model);
        Task<ServiceUserLoginResult> LoginAsync(ServiceUserLogin model);


        Task<bool> IsEmailTakenAsync(string email);
        Task<bool> SendEmailConfirmationAsync(string email);
        Task<bool> VerifyAsync(ServiceUserVerify serviceUserVerify);
    }

    public class UserService : IUserService
    {
        private readonly UserManager<DatabaseUser> userManager;
        private readonly IEmailService emailService;
        private readonly IJwtService jwtService;
        private readonly IConfiguration configuration;
        private readonly ICurve25519Service curve25519Service;
        private readonly IWavesAddressesNodeService wavesAddressesNodeService;
        private readonly IUserReferralLinkService userReferralLinkService;
        private readonly TokenaireContext tokenaireContext;

        public UserService(UserManager<DatabaseUser> userManager,
            IEmailService emailService,
            IJwtService jwtService,
            IConfiguration configuration,
            ICurve25519Service curve25519Service,
            IWavesAddressesNodeService wavesAddressesNodeService,
            IUserReferralLinkService userReferralLinkService,
            TokenaireContext tokenaireContext)
        {
            this.userManager = userManager;
            this.emailService = emailService;
            this.jwtService = jwtService;
            this.configuration = configuration;
            this.curve25519Service = curve25519Service;
            this.wavesAddressesNodeService = wavesAddressesNodeService;
            this.userReferralLinkService = userReferralLinkService;
            this.tokenaireContext = tokenaireContext;
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
                    ICOBTCAddress = dbUser.ICOBTCAddress,
                    ICOBTCRefundAddress = dbUser.ICOBTCRefundAddress,
                    RegisteredFromReferralLinkId = dbUser.RegistrationInfo.RegisteredFromReferralLink?.Id
                };
            }).ToList();
        }

        public async Task<ServiceUser> GetUser(string userId)
        {
            var dbUser = await this.userManager.Users
                .Include(u => u.RegistrationInfo)
                .Where(x => x.Id == userId)
                .FirstAsync();

            return new ServiceUser()
            {
                Id = dbUser.Id,
                Address = dbUser.Address,
                ICOBTCAddress = dbUser.ICOBTCAddress,
                ICOBTCRefundAddress = dbUser.ICOBTCRefundAddress,
                RegisteredFromReferralLinkId = dbUser.RegistrationInfo.RegisteredFromReferralLink?.Id
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
            var identity = await this.GetClaimsIdentityAsync(email, model.HashedPassword);
            if (identity == null)
            {
                return new ServiceUserLoginResult()
                {
                    Errors = new List<ServiceGenericError>() {
                        new ServiceGenericError()
                        {
                            Code = ServiceGenericErrorEnum.InvalidEmailOrPassword,
                            Message = "Invalid email or password"
                        }
                    }
                };
            }

            var user = await this.userManager.FindByEmailAsync(email);
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

            user.LastLoginDate = DateTime.UtcNow;
            await this.tokenaireContext.SaveChangesAsync();

            var jwt = await this.jwtService.GenerateJwt(identity, email);

            return new ServiceUserLoginResult()
            {
                Errors = new List<ServiceGenericError>(),
                EncryptedSeed = user.EncryptedSeed,
                Jwt = jwt,

                IsFirstTimeLogging = isFirstTimeLogging
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

        private async Task<ClaimsIdentity> GetClaimsIdentityAsync(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return null;

            var userToVerify = await this.userManager.FindByNameAsync(userName);
            if (userToVerify == null) return null;

            if (
                await this.userManager.CheckPasswordAsync(userToVerify, password))
            {
                return await jwtService.GenerateClaimsIdentity(userName, userToVerify.Id);
            }

            return null;
        }
    }
}

