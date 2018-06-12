using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Database.Models
{
    public class DatabaseIcoTransaction
    {
        public int Id { get; set; }

        public string UserId { get;  set; }
        public DatabaseUser user { get; set; }

        public string TxIdSource { get; set; }

        public string ICOBTCAddress { get; set; }

        public bool? IsSuccessful { get; set; }
        public string Content { get; set; }

        public bool IsProcessed { get; set; }
        public string ProcessType { get;  set; }

        public long ValueReceivedInSatoshies { get;  set; }
        public long ValueSentInAIRE { get; set; }

        public double OneAirePriceInSatoshies { get;  set; }

        public string RegisteredFromReferralLinkId { get; set; }
        public DatabaseUserReferralLink RegisteredFromReferralLink { get; set; }
    }

    public class DatabaseIcoTransactionConfig : IEntityTypeConfiguration<DatabaseIcoTransaction>
    {
        public void Configure(EntityTypeBuilder<DatabaseIcoTransaction> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.UserId).IsRequired();
            builder.HasIndex(i => new { i.TxIdSource, i.ICOBTCAddress }).IsUnique();
        }
    }
}