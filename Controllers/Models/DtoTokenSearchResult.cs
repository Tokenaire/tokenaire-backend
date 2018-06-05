using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace Tokenaire.Controllers.Models
{
    public class DtoTokenSearchResult
    {
        public string Id { get; set; }

        public string Image { get; set; }

        public string AmountAssetId { get; set; }
        public string PriceAssetId { get; set; }

        public bool IsGeneric { get; set; }
    }
}