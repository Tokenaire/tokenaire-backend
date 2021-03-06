﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tokenaire.Controllers.Models;
using Tokenaire.Database;
using Tokenaire.Database.Models;
using Tokenaire.Service;
using Tokenaire.Service.Enums;
using Tokenaire.Service.Models;
using tokenaire_backend.Extensions;

namespace tokenaire_backend.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IIcoService icoFundsService;
        private readonly IRecaptchaService recaptchaService;
        private readonly IWavesCoinomatService wavesCoinomatService;
        private readonly IIpService ipService;

        public UserController(
            IUserService userService,
            IIcoService icoFundsService,
            IRecaptchaService recaptchaService,
            IWavesCoinomatService wavesCoinomatService,
            IIpService ipService)
        {
            _userService = userService;
            this.icoFundsService = icoFundsService;
            this.recaptchaService = recaptchaService;
            this.wavesCoinomatService = wavesCoinomatService;
            this.ipService = ipService;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("isEmailTaken")]
        public async Task<IActionResult> IsEmailTaken(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest();
            }

            var isEmailTaken = await this._userService.IsEmailTakenAsync(email);

            return Ok(isEmailTaken);
        }

        [AllowAnonymous]
        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]DtoUserCreate model)
        {
            if (!await this.recaptchaService.IsValidCaptchaResponse(model.CaptchaResponse, this.ipService.GetClientIp()))
            {
                return this.BadRequestFromErrors(new List<ServiceGenericError>() {
                        new ServiceGenericError()
                        {
                            Code = ServiceGenericErrorEnum.InvalidCaptcha,
                            Message = "Invalid captcha"
                        }
                    });
            }

            var ICOBTCAddress = await this.icoFundsService.GenerateICOBtcAddressForUser(model?.Email);
            var BTCAddress = await this.wavesCoinomatService.GenerateBTCAddressFromWavesAddress(model?.Address);

            var serviceResult = await _userService.CreateAsync(new ServiceUserCreate()
            {
                Email = model?.Email,
                HashedPassword = model?.HashedPassword,
                EncryptedSeed = model?.EncryptedSeed,

                Address = model?.Address,
                BTCAddress = BTCAddress,
                PublicKey = model?.PublicKey,
                Signature = model?.Signature,

                ICOBTCAddress = ICOBTCAddress,

                RegisteredFromIP = this.ipService.GetClientIp(),
                RegisteredFromReferralLinkId = model.RegisteredFromReferralLinkId,
            });

            if (serviceResult.Errors.Count > 0)
            {
                return this.BadRequestFromErrors(serviceResult.Errors);
            }

            return Ok(new DtoUserCreateResult() { });
        }

        [Route("enableTwoFactorAuth")]
        [HttpPost]
        public async Task<IActionResult> EnableTwoFactorAuth([FromBody]DtoEnableTwoFactorAuth model)
        {
            if (model == null || string.IsNullOrEmpty(model.VerificationCode))
            {
                return BadRequest();
            }

            if (await this._userService.IsTwoFactorAuthEnabled(User.GetUserId()))
            {
                return BadRequest("two way auth already enabled");
            }

            var result = await this._userService.SetTwoFactorAuth(User.GetUserId(), model.VerificationCode, true);
            if (!result)
            {
                return BadRequest("could not enable two way auth");
            }

            return Ok();
        }

        [Route("getAuthenticatorKey")]
        [HttpPost]
        public async Task<IActionResult> GetAuthenticatorKey([FromBody]DtoAuthenticatorKey model)
        {
            if (model == null)
            {
                return BadRequest("model is null");
            }

            var authenticatorKey = await this._userService.GetAuthenticatorKeyAsync(User.GetUserId());
            return Ok(new DtoAuthenticatorKey() {
                Key = authenticatorKey.Key,
                KeyAsImage = authenticatorKey.KeyAsImage
            });
        }

        [Route("disableTwoFactorAuth")]
        [HttpPost]
        public async Task<IActionResult> DisableTwoFactorAuth([FromBody]DtoDisableTwoFactorAuth model)
        {
            if (model == null || string.IsNullOrEmpty(model.VerificationCode))
            {
                return BadRequest();
            }

            if (!await this._userService.IsTwoFactorAuthEnabled(User.GetUserId()))
            {
                return BadRequest("two way auth already disabled");
            }

            var result = await this._userService.SetTwoFactorAuth(User.GetUserId(), model.VerificationCode, false);
            if (!result)
            {
                return BadRequest("could not disable two way auth");
            }

            return Ok();
        }

        [AllowAnonymous]
        [Route("verify")]
        [HttpPost]
        public async Task<IActionResult> Verify([FromBody]DtoUserVerify model)
        {
            var isVerified = await _userService.VerifyAsync(new ServiceUserVerify()
            {
                Email = model.Email,
                Code = model.Code
            });

            return Ok(new DtoUserVerifyResult()
            {
                IsVerified = isVerified
            });
        }

        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody]DtoUserLogin model)
        {
            var serviceResult = await _userService.LoginAsync(new ServiceUserLogin()
            {
                Email = model?.Email,
                HashedPassword = model?.HashedPassword,
                VerificationCode = model?.VerificationCode
            });

            if (serviceResult.Errors.Count > 0)
            {
                return this.BadRequestFromErrors(serviceResult.Errors);
            }

            return Ok(new DtoUserLoginResult()
            {
                AuthToken = serviceResult.Jwt.AuthToken,
                EncryptedSeed = serviceResult.EncryptedSeed,
                IsFirstTimeLogging = serviceResult.IsFirstTimeLogging,
                IsTwoFactorAuthEnabled = serviceResult.IsTwoFactorAuthEnabled
            });
        }
    }
}
