using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Controllers.Models
{
    public class DtoTokenSearchResults
    {
        public List<DtoTokenSearchResult> AssetPairs { get; set; }
        public KeyValuePair<string, string> DefaultAssetIdPair { get; set; }
    }
}