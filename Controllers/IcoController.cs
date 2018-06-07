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
        private readonly IIcoService icoService;
        private readonly IUserReferralLinkService userReferralLinkService;
        private readonly IUserService userService;
        private readonly IIpService ipService;
        private readonly ISettingsService settingsService;
        private readonly IMathService mathService;
        private readonly IConfiguration configuration;

        public IcoController(
            IIcoService icoFundsService,
            IUserReferralLinkService userReferralLinkService,
            IUserService userService,
            IIpService ipService,
            ISettingsService settingsService,
            IMathService mathService,
            IConfiguration configuration)
        {
            this.icoService = icoFundsService;
            this.userReferralLinkService = userReferralLinkService;
            this.userService = userService;
            this.ipService = ipService;
            this.settingsService = settingsService;
            this.mathService = mathService;
            this.configuration = configuration;
        }

        [Route("setRefundAddress")]
        [HttpPost]
        public async Task<IActionResult> SetRefundAddress([FromBody]DtoSetRefundAddress model) 
        {
            await this.icoService.SetRefundAddress(User.GetUserId(), model?.BTCAddress);
            return Ok();
        }

        [Route("mydetails")]
        [HttpGet]
        public async Task<IActionResult> MyDetails()
        {
            var platformUrl = this.configuration.GetValue<string>("TokenairePlatformUrl");
            var icoDetails = await this.icoService.GetMyICODetails(User.GetUserId());
            var isIcoRunning = await this.icoService.IsICORunning();

            return Ok(new DtoIcoMyDetailsResult() {
                ICOBTCAddress = isIcoRunning ? icoDetails.ICOBTCAddress : null,
                ICOBTCRefundAddress = icoDetails.ICOBTCRefundAddress,

                ICOBTCInvested =  this.mathService.ConvertSatoshiesToBTCFormatted(icoDetails.ICOBTCInvestedSatoshies),
                ReferralLinkUrl = icoDetails.ReferralLinkUrl,
                ReferralLinkRaisedBtc = this.mathService.ConvertSatoshiesToBTCFormatted(icoDetails.ReferralLinkRaisedBtcSatoshies),
                ReferralLinkEligibleBtc = this.mathService.ConvertSatoshiesToBTCFormatted(icoDetails.ReferralLinkEligibleBtcSatoshies),

                OneAireInSatoshies = icoDetails.OneAireInSatoshies,

                AireToReceive = icoDetails.AIREToReceive
            });
        }

        [AllowAnonymous]
        [Route("Info")]
        [HttpGet]
        public async Task<IActionResult> GetInfo()
        {
            var tokensSold = await this.icoService.GetAIRESold();
            var tokensAvailable = await this.icoService.GetAIRELeft();
            var tokenSaleSupply = tokensSold + tokensAvailable;

            return Ok(new {
                TokensSold = tokensSold,
                TokensAvailable = tokensAvailable,
                TokenSaleSupply = tokenSaleSupply,
                TokensSoldPercentage = tokensSold * 100 / tokenSaleSupply
            });
        }

        [AllowAnonymous]
        [Route("ProcessFunds")]
        [HttpPost]
        public async Task<IActionResult> ProcessFunds([FromBody]DtoIcoProcessFunds model)
        {
            if (model?.SecretPassword != this.settingsService.BackendPassword) {
                return BadRequest();
            }

            await this.icoService.ProcessFunds();
            return Ok();
        }

        [AllowAnonymous]
        [Route("ProcessKYC")]
        [HttpPost]
        public async Task<IActionResult> ProcessKYC([FromQuery(Name = "key")] string key, [FromBody]DtoIcoProcessKYC model)
        {
            var requestIP = this.ipService.GetClientIp();
            var allowedIPS = new string[]{
                "127.0.0.1",
                "::1"
            };

            if (!allowedIPS.Contains(requestIP)) {
                return BadRequest();
            }

            if (key != this.settingsService.SumSubHookKey) {
                return BadRequest();
            }

            await this.icoService.ProcessKyc(new ServiceIcoProcessKyc() {
                ApplicantId = model.ApplicantId,
                InspectionId = model.InspectionId,
                Success = model.Success,
                CorrelationId = model.CorrelationId,
                ExternalUserId = model.ExternalUserId,
                Details = model.Details,
                Type = model.Type,

                Review = new ServiceIcoProcessKycReview() {
                    ModerationComment = model.Review?.ModerationComment,
                    ClientComment = model.Review?.ClientComment,
                    ReviewAnswer = model.Review?.ReviewAnswer,
                    RejectLabels = model.Review?.RejectLabels,
                    ReviewRejectType = model.Review?.ReviewRejectType
                }
            });
            
            return Ok();
        }
    }
}
