using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tokenaire.Controllers.Models;
using Tokenaire.Service;
using Tokenaire.Service.Models;

namespace tokenaire_backend.Controllers
{
    [Route("api/[controller]")]
    public class EmailController : Controller
    {
        private readonly IEmailSubscriptionService emailSubscriptionService;
        private readonly IIpService ipService;

        public EmailController(IEmailSubscriptionService emailSubscriptionService, IIpService ipService)
        {
            this.emailSubscriptionService = emailSubscriptionService;
            this.ipService = ipService;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]DtoEmailCreate model)
        {
            var serviceResult = await this.emailSubscriptionService.Create(new ServiceEmailCreateSubscription()
            {
                Value = model?.Value,
                Ip = this.ipService.GetClientIpXForward()
            });

            if (serviceResult.Errors.Count > 0) {
                return this.BadRequestFromErrors(serviceResult.Errors);
            }

            return Ok();            
        }
    }
}
