using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tokenaire.Database.Models
{
    public enum DatabaseUserReferralLinkType
    {
        ICO = 1
    }

    public class DatabaseUserReferralLink
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public DatabaseUser User { get; set; }
        public DatabaseUserReferralLinkType? Type { get; set; }
    }

    public class DatabaseUserReferralLinkConfig : IEntityTypeConfiguration<DatabaseUserReferralLink>
    {
        public void Configure(EntityTypeBuilder<DatabaseUserReferralLink> builder)
        {
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.Type).IsRequired();

        }
    }
}