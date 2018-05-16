using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Database.Models
{
    public class DatabaseEmail
    {
        public int Id { get; set; }

        public string Value { get; set; }

        public string Ip { get; set; }

        public DateTime Date { get; set; }
    }

    public class DatabaseEmailConfig : IEntityTypeConfiguration<DatabaseEmail>
    {
        public void Configure(EntityTypeBuilder<DatabaseEmail> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Value).IsUnique();
        }
    }
}