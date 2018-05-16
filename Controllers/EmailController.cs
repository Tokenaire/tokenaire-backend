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
        private readonly IEmailService emailService;
        private readonly IHttpContextAccessor accessor;

        public EmailController(IEmailService emailService, IHttpContextAccessor accessor)
        {
            this.emailService = emailService;
            this.accessor = accessor;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]DtoEmailCreate model)
        {
            var serviceResult = await this.emailService.Create(new ServiceEmailCreate()
            {
                Value = model?.Value,
                Ip = this.accessor.HttpContext.Connection.RemoteIpAddress.ToString()
            });

            if (serviceResult.Errors.Count > 0) {
                return this.BadRequestFromErrors(serviceResult.Errors);
            }

            return Ok();            
        }
    }
}
