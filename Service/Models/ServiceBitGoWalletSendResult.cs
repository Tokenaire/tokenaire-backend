using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tokenaire.Service.Models
{
    public class ServiceBitGoWalletSendResult
    {
        public bool IsSuccessful { get; set; }
        public string Content { get; set; }

        public string TxId { get; set; }
        public string Status { get; set; }
    }
}