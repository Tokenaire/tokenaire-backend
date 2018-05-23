using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Database.Models
{
    public class DatabaseUserRegistrationInfo
    {
        public string UserId { get; set; }
        public DatabaseUser User { get; set; }

        public string RegisteredFromReferralLinkId { get; set; }
        public DatabaseUserReferralLink RegisteredFromReferralLink { get; set; }
    }

    public class DatabaseUserRegistrationInfoConfig : IEntityTypeConfiguration<DatabaseUserRegistrationInfo>
    {
        public void Configure(EntityTypeBuilder<DatabaseUserRegistrationInfo> builder)
        {
            builder.HasKey(x => x.UserId);
            builder.HasOne(p => p.User)
                   .WithOne()
                   .HasForeignKey<DatabaseUser>(pe => pe.Id);
        }
    }
}