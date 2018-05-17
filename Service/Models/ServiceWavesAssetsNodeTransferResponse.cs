using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tokenaire.Service.Models
{
    public class ServiceWavesAssetsNodeTransferResponse
    {
        public bool IsSuccessful { get; set; }
        public string Content { get; set; }
    }
}