using System;
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
using Tokenaire.Service.Models;

namespace tokenaire_backend.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IIcoService icoFundsService;
        private readonly IWavesCoinomatService wavesCoinomatService;
        private readonly IIpService ipService;

        public UserController(
            IUserService userService,
            IIcoService icoFundsService,
            IWavesCoinomatService wavesCoinomatService,
            IIpService ipService)
        {
            _userService = userService;
            this.icoFundsService = icoFundsService;
            this.wavesCoinomatService = wavesCoinomatService;
            this.ipService = ipService;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("isEmailTaken")]
        public async Task<IActionResult> IsEmailTaken(string email) {
            if (string.IsNullOrEmpty(email)) {
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
            var ICOBTCAddress = await this.icoFundsService.GenerateICOBtcAddressForUser(model?.Email);
            var serviceResult = await _userService.CreateAsync(new ServiceUserCreate()
            {
                Email = model?.Email,
                HashedPassword = model?.HashedPassword,
                EncryptedSeed = model?.EncryptedSeed,

                Address = model?.Address,
                PublicKey = model?.PublicKey,
                Signature = model?.Signature,

                ICOBTCAddress = ICOBTCAddress,

                RegisteredFromIP = this.ipService.GetClientIpXForward(),
                RegisteredFromReferralLinkId = model.RegisteredFromReferralLinkId,
            });

            if (serviceResult.Errors.Count > 0)
            {
                return this.BadRequestFromErrors(serviceResult.Errors);
            }

            return Ok(new DtoUserCreateResult() {});
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

            return Ok(new DtoUserVerifyResult() {
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
            });

            if (serviceResult.Errors.Count > 0)
            {
                return this.BadRequestFromErrors(serviceResult.Errors);
            }

            return Ok(new DtoUserLoginResult() {
                AuthToken = serviceResult.Jwt.AuthToken,
                EncryptedSeed = serviceResult.EncryptedSeed,
                IsFirstTimeLogging = serviceResult.IsFirstTimeLogging
            });
        }
    }
}
