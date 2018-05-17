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
        private readonly IIcoFundsService icoFundsService;

        public UserController(IUserService userService, IIcoFundsService icoFundsService)
        {
            _userService = userService;
            this.icoFundsService = icoFundsService;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("isEmailTaken")]
        public async Task<IActionResult> IsEmailTaken(string email) {
            if (string.IsNullOrEmpty(email)) {
                return BadRequest();
            }

            var isEmailTaken = await this._userService.IsEmailTaken(email);

            return Ok(isEmailTaken);
        }

        [AllowAnonymous]
        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]DtoUserCreate model)
        {
            var ICOBTCAddress = await this.icoFundsService.GenerateICOBtcAddressForUser(model?.Email);
            var serviceResult = await _userService.Create(new ServiceUserCreate()
            {
                Email = model?.Email,
                HashedPassword = model?.HashedPassword,
                EncryptedSeed = model?.EncryptedSeed,

                Address = model?.Address,
                PublicKey = model?.PublicKey,
                Signature = model?.Signature,

                ICOBTCAddress = ICOBTCAddress
            });

            if (serviceResult.Errors.Count > 0)
            {
                return this.BadRequestFromErrors(serviceResult.Errors);
            }

            return Ok(new DtoUserCreateResult() {
                AuthToken = serviceResult.Jwt.AuthToken,
                ICOBTCAddress = ICOBTCAddress
            });
        }

        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody]DtoUserLogin model)
        {
            var serviceResult = await _userService.Login(new ServiceUserLogin()
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
                ICOBTCAddress = serviceResult.ICOBTCAddress
            });
        }
    }
}
