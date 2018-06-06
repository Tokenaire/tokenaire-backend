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

namespace tokenaire_backend.Controllers
{
    [Route("api/[controller]")]
    public class TokenController : Controller
    {
        private readonly IConfiguration configuration;

        public TokenController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [AllowAnonymous]
        [Route("search")]
        [HttpPost]
        public async Task<IActionResult> Search([FromBody]DtoTokenSearch model)
        {
            var aireTokenAssetId = this.configuration.GetValue<string>("AIRETokenAssetId");
            var btcAssetId = "8LQW8f7P5d5PZM7GtZEBgaqRPGSzS3DfPuiXrURJ4AJS";

            var chrisEelmaaAssetId = "2ATzTjhsgkswXbnMGfucnto1fj6E9mXEQiC3jF6zpWoC";
            var joyceAssetId = "HyEhnk91FYXsbj1sYXWT5BvRqi256dx7kA4W8A5DkCvV";
            var reinhardAssetId = "G6ZGKQ18EeMhBTiUBh9dHPurxZDsaCQvincdwTws2AwA";

            return Ok(new DtoTokenSearchResults()
            {
                DefaultAssetIdPair = new KeyValuePair<string, string>(aireTokenAssetId, "WAVES"),
                AssetPairs = new List<DtoTokenSearchResult>() {
                 new DtoTokenSearchResult() {
                        Id = Guid.NewGuid().ToString(),
                        Image = "/img/icons/dex-p-waves-btc.svg",

                        AmountAssetId = "WAVES",
                        PriceAssetId = btcAssetId,

                        IsGeneric = true
                    },

                    new DtoTokenSearchResult() {
                        Id = Guid.NewGuid().ToString(),
                        Image = "/img/icons/dex-p-aire-btc.svg",

                        AmountAssetId = aireTokenAssetId,
                        PriceAssetId = btcAssetId,

                        IsGeneric = true
                    },

                    new DtoTokenSearchResult() {
                        Id = Guid.NewGuid().ToString(),
                        Image = "/img/icons/dex-p-aire-waves.svg",

                        AmountAssetId = aireTokenAssetId,
                        PriceAssetId = "WAVES",

                        IsGeneric = true
                    },



                    new DtoTokenSearchResult() {
                        Id = Guid.NewGuid().ToString(),
                        Image = "/img/icons/chris-avatar.jpg",
                        Name = "Erti-Chris Eelmaa",

                        AmountAssetId = aireTokenAssetId,
                        PriceAssetId = chrisEelmaaAssetId,

                        IsGeneric = false,
                        ShowDivider = true,
                    },

                    new DtoTokenSearchResult() {
                        Id = Guid.NewGuid().ToString(),
                        Image = "/img/icons/joyce-avatar.jpg",
                        Name = "Joyce Man",

                        AmountAssetId = aireTokenAssetId,
                        PriceAssetId = joyceAssetId,

                        IsGeneric = false,
                        ShowDivider = true,
                    },

                    new DtoTokenSearchResult() {
                        Id = Guid.NewGuid().ToString(),
                        Image = "/img/icons/reinhard-avatar.jpg",
                        Name = "Reinhard Zach",

                        AmountAssetId = aireTokenAssetId,
                        PriceAssetId = reinhardAssetId,

                        IsGeneric = false,
                        ShowDivider = true,
                    },




                }
            });
        }
    }
}
