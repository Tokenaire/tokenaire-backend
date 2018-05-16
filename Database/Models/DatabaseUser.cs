using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Database.Models
{
    public class DatabaseUser : IdentityUser
    {
        public string EncryptedSeed { get; set; }

        public string Address { get; set; }
        public string PublicKey { get; set; }
        public string Signature { get; set; }
    }

    public class DatabaseUserConfig : IEntityTypeConfiguration<DatabaseUser>
    {
        public void Configure(EntityTypeBuilder<DatabaseUser> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.EncryptedSeed).IsRequired();
            builder.Property(x => x.Address).IsRequired();
            builder.Property(x => x.PublicKey).IsRequired();
            builder.Property(x => x.Signature).IsRequired();
        }
    }
}