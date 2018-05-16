using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tokenaire.Database.Models;

namespace Tokenaire.Database
{
    public static class TokenaireContextExtensions
    {
        public static void AddTokenaireContext(this IServiceCollection services, IConfiguration configuration) {
            var dbString = configuration.GetConnectionString("TokenaireDatabase");
            services.AddDbContext<TokenaireContext>(options => options.UseSqlServer(dbString));
        }
    }
}

