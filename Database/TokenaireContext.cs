using Microsoft.EntityFrameworkCore;
using Tokenaire.Database.Models;

namespace Tokenaire.Database
{

    public class TokenaireContext : DbContext
    {
        public DbSet<DatabaseEmail> Emails { get; set; }
        public DbSet<DatabaseUser> Users { get; set; }

        public TokenaireContext(DbContextOptions<TokenaireContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new DatabaseEmailConfig());
            modelBuilder.ApplyConfiguration(new DatabaseUserConfig());

        }
    }
}