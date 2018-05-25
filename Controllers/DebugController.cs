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
    public class DebugController : Controller
    {
        private readonly IIcoService icoFundsService;

        public DebugController(IIcoService icoFundsService)
        {
            this.icoFundsService = icoFundsService;
        }
    }
}
