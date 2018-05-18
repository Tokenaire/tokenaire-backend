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
    public class IcoController : Controller
    {
        private readonly IIcoFundsService icoFundsService;

        public IcoController(IIcoFundsService icoFundsService)
        {
            this.icoFundsService = icoFundsService;
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
