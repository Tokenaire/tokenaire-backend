using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
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
        Task<List<ServiceUser>> GetUsers();

        Task<ServiceUserCreateResult> Create(ServiceUserCreate model);
        Task<ServiceUserLoginResult> Login(ServiceUserLogin model);


        Task<bool> IsEmailTaken(string email);

    }

    public class UserService : IUserService
    {
        private readonly UserManager<DatabaseUser> userManager;
        private readonly IEmailService emailService;
        private readonly IJwtService jwtService;
        private readonly ICurve25519Service curve25519Service;
        private readonly IWavesAddressesNodeService wavesAddressesNodeService;

        public UserService(UserManager<DatabaseUser> userManager,
            IEmailService emailService,
            IJwtService jwtService,
            ICurve25519Service curve25519Service,
            IWavesAddressesNodeService wavesAddressesNodeService)
        {
            this.userManager = userManager;
            this.emailService = emailService;
            this.jwtService = jwtService;
            this.curve25519Service = curve25519Service;
            this.wavesAddressesNodeService = wavesAddressesNodeService;
        }

        public async Task<List<ServiceUser>> GetUsers()
        {
            var dbUsers = await this.userManager.Users.ToArrayAsync();
            return dbUsers.Select((dbUser) =>
            {
                return new ServiceUser()
                {
                    Id = dbUser.Id,
                    Address = dbUser.Address,
                    ICOBTCAddress = dbUser.ICOBTCAddress
                };
            }).ToList();
        }

        public async Task<ServiceUserCreateResult> Create(ServiceUserCreate model)
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

            var result = await this.userManager.CreateAsync(new DatabaseUser()
            {
                UserName = email,
                Email = email,
                EncryptedSeed = model.EncryptedSeed,

                Address = model.Address,
                PublicKey = model.PublicKey,
                Signature = model.Signature,

                ICOBTCAddress = model.ICOBTCAddress
            },
            model.HashedPassword);

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

            var identity = await this.GetClaimsIdentity(email, model.HashedPassword);
            var jwt = await this.jwtService.GenerateJwt(identity, email);

            return new ServiceUserCreateResult()
            {
                Errors = new List<ServiceGenericError>(),
                Jwt = jwt
            };
        }

        public async Task<bool> IsEmailTaken(string email)
        {
            return await this.userManager.FindByEmailAsync(email) != null;
        }

        public async Task<ServiceUserLoginResult> Login(ServiceUserLogin model)
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
            var identity = await this.GetClaimsIdentity(email, model.HashedPassword);
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

            var jwt = await this.jwtService.GenerateJwt(identity, email);
            var user = await this.userManager.FindByNameAsync(email);

            return new ServiceUserLoginResult()
            {
                Errors = new List<ServiceGenericError>(),
                EncryptedSeed = user.EncryptedSeed,
                Jwt = jwt,

                ICOBTCAddress = user.ICOBTCAddress
            };
        }

        private async Task<ClaimsIdentity> GetClaimsIdentity(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return null;

            var userToVerify = await this.userManager.FindByNameAsync(userName);
            if (userToVerify == null) return null;

            if (await this.userManager.CheckPasswordAsync(userToVerify, password))
            {
                return await jwtService.GenerateClaimsIdentity(userName, userToVerify.Id);
            }

            return null;
        }
    }
}

