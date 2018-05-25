using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Database.Models
{
    public class DatabaseIcoTransactionProcessHistory
    {
        public int Id { get; set; }

        public int IcoTransactionId { get; set; }

        public DatabaseIcoTransaction IcoTransaction { get; set; }

        public string Content { get; set; }
    }

    public class DatabaseIcoTransactionProcessHistoryConfig : IEntityTypeConfiguration<DatabaseIcoTransactionProcessHistory>
    {
        public void Configure(EntityTypeBuilder<DatabaseIcoTransactionProcessHistory> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.IcoTransactionId).IsRequired();
        }
    }
}