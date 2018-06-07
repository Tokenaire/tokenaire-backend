using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Database.Models
{
    public class DatabaseIcoKyc
    {
        public int Id { get; set; }

        public string UserId { get;  set; }
        public DatabaseUser user { get; set; }

        public bool IsSuccessful { get; set; }
        public string Content { get; set; }

    }

    public class DatabaseIcoKycConfig : IEntityTypeConfiguration<DatabaseIcoKyc>
    {
        public void Configure(EntityTypeBuilder<DatabaseIcoKyc> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(p => new { p.UserId, p.IsSuccessful});
            builder.Property(x => x.UserId).IsRequired();
        }
    }
}