using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tokenaire.Service.Models
{
    public class ServiceWavesAssetsNodeTransfer
    {
        public string PrivateKey { get; set; }
        public string AssetId { get; set; }
        public string FeeAssetId { get; set; }
        public string Attachment { get; set; }
        public string ToAddress { get; set; }

        public long Fee { get; set; }
        public long Amount { get; set; }
    }
}