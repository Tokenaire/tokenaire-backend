using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Tokenaire.Database.Models;

namespace Tokenaire.Database
{

    public class TokenaireContext : DbContext
    {
        public DbSet<DatabaseEmail> Emails { get; set; }
        public DbSet<DatabaseUser> Users { get; set; }
        public DbSet<DatabaseIcOOutboundAIRETransaction> ICOOutboundAIRETransactions { get; set; }

        public TokenaireContext(DbContextOptions<TokenaireContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.ReplaceService<IEntityMaterializerSource, CustomEntityMaterializerSource>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new DatabaseEmailConfig());
            modelBuilder.ApplyConfiguration(new DatabaseUserConfig());
            modelBuilder.ApplyConfiguration(new DatabaseICOOutboundAIRETransactionConfig());


            MethodInfo methodInfo = typeof(TokenaireContext)
                   .GetRuntimeMethod(nameof(DateDiff), new[] { typeof(string), typeof(DateTime), typeof(DateTime) });

            modelBuilder
                .HasDbFunction(methodInfo)
                .HasTranslation(args =>
                    {
                        var arr = args.ToArray();
                        var diffArgs = new[] {
                            new SqlFragmentExpression(arr[0].ToString()),
                            arr[1],
                            arr[2]
                        };

                        return new SqlFunctionExpression(
                            "datediff",
                            methodInfo.ReturnType,
                            diffArgs);
                    }
                );
        }

        [DbFunction]
        public static int DateDiff(string datepart, DateTime startdate, DateTime enddate)
        {
            throw new Exception();
        }
    }
}