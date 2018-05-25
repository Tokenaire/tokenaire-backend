using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tokenaire.Service.Models
{
    public class ServiceBitGoWalletSend
    {
        public long AmountInSatoshies { get; set; }
        public string Address { get; set; }

        public string WalletId { get; set; }
        public string WalletPassword { get; set; }
    }
}