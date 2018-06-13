using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tokenaire.Controllers.Models;
using Tokenaire.Service;
using Tokenaire.Service.Enums;
using Tokenaire.Service.Models;
using tokenaire_backend.Extensions;

namespace tokenaire_backend.Controllers
{
    [Route("api/[controller]")]
    public class IcoController : Controller
    {
        private readonly IIcoService icoService;
        private readonly IIcoKycService icoKycService;
        private readonly IUserReferralLinkService userReferralLinkService;
        private readonly IUserService userService;
        private readonly ISumSubApiService sumSubApiService;
        private readonly IIpService ipService;
        private readonly ISettingsService settingsService;
        private readonly IMathService mathService;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly IConfiguration configuration;

        public IcoController(
            IIcoService icoFundsService,
            IIcoKycService icoKycService,
            IUserReferralLinkService userReferralLinkService,
            IUserService userService,
            ISumSubApiService sumSubApiService,
            IIpService ipService,
            ISettingsService settingsService,
            IMathService mathService,
            IHostingEnvironment hostingEnvironment,
            IConfiguration configuration)
        {
            this.icoService = icoFundsService;
            this.icoKycService = icoKycService;
            this.userReferralLinkService = userReferralLinkService;
            this.userService = userService;
            this.sumSubApiService = sumSubApiService;
            this.ipService = ipService;
            this.settingsService = settingsService;
            this.mathService = mathService;
            this.hostingEnvironment = hostingEnvironment;
            this.configuration = configuration;
        }

        [Route("setRefundAddress")]
        [HttpPost]
        public async Task<IActionResult> SetRefundAddress([FromBody]DtoSetRefundAddress model)
        {
            await this.icoService.SetRefundAddress(User.GetUserId(), model?.BTCAddress);
            return Ok();
        }

        [Route("createSumSubAccessToken")]
        [HttpPost]
        public async Task<IActionResult> CreateSumSubAccessToken([FromBody]DtoSetRefundAddress model)
        {
            var accessToken = await this.sumSubApiService.CreateIFrameAccessToken(User.GetUserId());
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest();
            }

            return Ok(new
            {
                AccessToken = accessToken
            });
        }

        [Route("mydetails")]
        [HttpGet]
        public async Task<IActionResult> MyDetails()
        {
            var platformUrl = this.configuration.GetValue<string>("TokenairePlatformUrl");
            var icoDetails = await this.icoService.GetMyICODetails(User.GetUserId());
            var icoStatus = await this.icoService.GetICOStatus();

            if (icoStatus == ServiceIcoStatus.Finished) {
                return Ok(new DtoIcoMyDetailsResult()
                {
                    ICOStatus = (int)icoStatus
                });
            }

            // if user is not KYCED
            // we'll not return anything to him yet.
            if (!icoDetails.IsKyced)
            {
                return Ok(new DtoIcoMyDetailsResult()
                {
                    IsKyced = false,
                    ICOStatus = (int)icoStatus
                });
            }

            // ICO not yet started,
            // however user can see referral link status.
            if (icoStatus == ServiceIcoStatus.NotYetStarted)
            {
                return Ok(new DtoIcoMyDetailsResult()
                {
                    IsKyced = true,
                    ICOStatus = (int)icoStatus,

                    ReferralLinkUrl = icoDetails.ReferralLinkUrl,
                    ReferralLinkUsedByPeople = icoDetails.ReferralLinkUsedByPeople,
                    ReferralLinkRaisedBtc = this.mathService.ConvertSatoshiesToBTCFormatted(icoDetails.ReferralLinkRaisedBtcSatoshies),
                    ReferralLinkEligibleBtc = this.mathService.ConvertSatoshiesToBTCFormatted(icoDetails.ReferralLinkEligibleBtcSatoshies),
                });
            }

            // ICO running & KYCED
            // send out all the details
            return Ok(new DtoIcoMyDetailsResult()
            {
                IsKyced = true,
                ICOStatus = (int)icoStatus,

                ICOBTCAddress = icoDetails.ICOBTCAddress,
                ICOBTCRefundAddress = icoDetails.ICOBTCRefundAddress,
                ICOBTCInvested = this.mathService.ConvertSatoshiesToBTCFormatted(icoDetails.ICOBTCInvestedSatoshies),

                ReferralLinkUrl = icoDetails.ReferralLinkUrl,
                ReferralLinkUsedByPeople = icoDetails.ReferralLinkUsedByPeople,
                ReferralLinkRaisedBtc = this.mathService.ConvertSatoshiesToBTCFormatted(icoDetails.ReferralLinkRaisedBtcSatoshies),
                ReferralLinkEligibleBtc = this.mathService.ConvertSatoshiesToBTCFormatted(icoDetails.ReferralLinkEligibleBtcSatoshies),

                OneAireInSatoshies = icoDetails.OneAireInSatoshies,

                AireToReceive = icoDetails.AIREToReceive,
                DiscountRate = icoDetails.DiscountRate
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

            return Ok(new
            {
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
            if (model?.SecretPassword != this.settingsService.BackendPassword)
            {
                return BadRequest();
            }

            await this.icoService.ProcessFunds();
            return Ok();
        }

        [AllowAnonymous]
        [Route("ProcessKYC")]
        [HttpPost]
        public async Task<IActionResult> ProcessKYC()
        {
            var key = Request.Query["key"];
            var digest = Request.Query["digest"];
            var requestIP = this.ipService.GetClientIp();
            var allowedIPS = new string[]{
                "127.0.0.1",
                "::1",
                "188.93.22.76"
            };

            if (!allowedIPS.Contains(requestIP))
            {
                return BadRequest(requestIP + " not allowed to access this resource");
            }

            if (key != this.settingsService.SumSubHookKey)
            {
                return BadRequest("invalid sumsubhookkey:" + key);
            }

            dynamic dynamicModel = null;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var rawData = await reader.ReadToEndAsync();
                var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(settingsService.SumSubSignKey));
                var hashmessage = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                var digestValidation = hashmessage.ToHexString();

                if (digest != digestValidation)
                {
                    return BadRequest("digest validation invalid");
                }

                dynamicModel = JObject.Parse(rawData);
            }

            // for now,
            // we are not interested in other types of messages.
            if (dynamicModel.type != "INSPECTION_REVIEW_COMPLETED")
            {
                return Ok();
            }

            var model = dynamicModel.ToObject(typeof(DtoIcoProcessKYC));
            await this.icoKycService.ProcessKyc(new ServiceIcoProcessKyc()
            {
                ApplicantId = model.ApplicantId,
                InspectionId = model.InspectionId,
                IsSuccess = model.Review?.ReviewAnswer == "GREEN",
                CorrelationId = model.CorrelationId,
                ExternalUserId = model.ExternalUserId,
                Details = model.Details,
                Type = model.Type,

                Review = new ServiceIcoProcessKycReview()
                {
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
