using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Database.Models
{
    public class DatabaseIcOOutboundAIRETransaction
    {
        public int Id { get; set; }

        public string TxIdSource { get; set; }

        public string AddressSource { get; set; }

        public bool? IsSuccessful { get; set; }
        public string Content { get; set; }
    }

    public class DatabaseICOOutboundAIRETransactionConfig : IEntityTypeConfiguration<DatabaseIcOOutboundAIRETransaction>
    {
        public void Configure(EntityTypeBuilder<DatabaseIcOOutboundAIRETransaction> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(i => new { i.TxIdSource, i.AddressSource }).IsUnique();
        }
    }
}