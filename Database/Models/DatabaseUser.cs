using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Database.Models
{
    public class DatabaseUser : IdentityUser
    {
        public string EncryptedSeed { get; set; }

        public string BTCAddress { get; set; }
        public string Address { get; set; }
        public string PublicKey { get; set; }
        public string Signature { get; set; }

        public string ICOBTCAddress { get; set; }
        public string ICOBTCRefundAddress { get; set; }
        public bool ICOKyced { get; set;}

        public string RegisteredFromIP { get; set; }

        public DateTime RegisteredDate { get; set; }
        public DateTime? LastLoginDate { get; set; }

        public DatabaseUserRegistrationInfo RegistrationInfo { get; set; }
        public List<DatabaseUserReferralLink> ReferralLinks { get; set; }
    }

    public class DatabaseUserConfig : IEntityTypeConfiguration<DatabaseUser>
    {
        public void Configure(EntityTypeBuilder<DatabaseUser> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.EncryptedSeed).IsRequired();
            builder.Property(x => x.Address).IsRequired();
            builder.Property(x => x.BTCAddress).IsRequired();

            builder.Property(x => x.PublicKey).IsRequired();
            builder.Property(x => x.Signature).IsRequired();
            builder.Property(x => x.RegisteredFromIP).IsRequired();
            builder.Property(x => x.RegisteredDate).IsRequired();

            builder.Property(x => x.ICOBTCAddress).IsRequired();
        }
    }
}