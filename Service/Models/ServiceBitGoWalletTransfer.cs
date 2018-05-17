using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tokenaire.Service.Models
{
    public class ServiceBitGoWalletTransferEntry
    {
        public string Address { get; set; }
        public string Wallet { get; set; }

        public long Value { get; set; }
    }

    public class ServiceBitGoWalletTransfer
    {
        public string TxId { get; set; }
        public string State { get; set; }

        public long Value { get; set; }
        public long Confirmations { get; set; }

        public List<ServiceBitGoWalletTransferEntry> Entries { get; set; }

        public double Usd { get; set; }
    }
}