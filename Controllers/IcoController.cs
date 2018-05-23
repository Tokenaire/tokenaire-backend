using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Tokenaire.Controllers.Models;
using Tokenaire.Service;
using Tokenaire.Service.Models;
using tokenaire_backend.Extensions;

namespace tokenaire_backend.Controllers
{
    [Route("api/[controller]")]
    public class IcoController : Controller
    {
        private readonly IIcoFundsService icoFundsService;
        private readonly IUserReferralLinkService userReferralLinkService;
        private readonly IUserService userService;
        private readonly IConfiguration configuration;

        public IcoController(
            IIcoFundsService icoFundsService,
            IUserReferralLinkService userReferralLinkService,
            IUserService userService,
            IConfiguration configuration)
        {
            this.icoFundsService = icoFundsService;
            this.userReferralLinkService = userReferralLinkService;
            this.userService = userService;
            this.configuration = configuration;
        }

        [Route("mydetails")]
        [HttpGet]
        public async Task<IActionResult> MyDetails()
        {
            var platformUrl = this.configuration.GetValue<string>("TokenairePlatformUrl");
            var icoDetails = await this.icoFundsService.GetMyICODetails(User.GetUserId());
            var AIREBalance = await this.icoFundsService.GetAIREBalance();

            var showIcoBTCAddress = AIREBalance != null && AIREBalance > 10000000;

            return Ok(new DtoIcoMyDetailsResult() {
                ICOBTCAddress = showIcoBTCAddress ? icoDetails.ICOBTCAddress : null,
                ICOBTCInvested = icoDetails.ICOBTCInvested,
                ReferralLinkUrl = icoDetails.ReferralLinkUrl,
                ReferralLinkRaisedBtc = icoDetails.ReferralLinkRaisedBtc,
                ReferralLinkEligibleBtc = icoDetails.ReferralLinkEligibleBtc,

                AireBalance = AIREBalance
            });
        }

        [AllowAnonymous]
        [Route("Balance")]
        [HttpGet]
        public async Task<IActionResult> GetAIREBalance()
        {
            return Ok(new {
                Balance = await this.icoFundsService.GetAIREBalance()
            });
        }

        [AllowAnonymous]
        [Route("ProcessFunds")]
        [HttpPost]
        public async Task<IActionResult> ProcessFunds([FromBody]DtoIcoProcessFunds model)
        {
            if (model?.SecretPassword != "mysuperskop8329xxkop") {
                return BadRequest();
            }

            await this.icoFundsService.ProcessFunds();
            return Ok();
        }
    }
}
