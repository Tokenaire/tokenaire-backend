using System;
using DnsClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tokenaire.Database;
using Tokenaire.Database.Models;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tokenaire.Service.Models.Models;
using Tokenaire.Service.Models;
using RestSharp;
using Loggly;
using AspNetCoreRateLimit;

namespace Tokenaire.Service
{
    public static class ServiceExtensions
    {
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IIcoService, IcoService>();
            services.AddScoped<IEmailSubscriptionService, EmailSubscriptionService>();
            services.AddScoped<IUserReferralLinkService, UserReferralLinkService>();



            services.AddSingleton<IJwtService, JwtService>();
            services.AddSingleton<IRestClient, RestClient>();
            services.AddSingleton<ICurve25519Service, Curve25519Service>();
            services.AddSingleton<IEmailService, EmailService>();
            services.AddSingleton<IMathService, MathService>();


            services.AddSingleton<IWavesRootNodeService, WavesRootNodeService>();
            services.AddSingleton<IWavesAddressesNodeService, WavesAddressesNodeService>();
            services.AddSingleton<IWavesAssetsNodeService, WavesAssetsNodeService>();
            services.AddTransient<IWavesNodeRestClientService, WavesNodeRestClientService>();

            services.AddSingleton<ILogglyClient, LogglyClient>();
            services.AddSingleton<ILogService, LogService>();
            services.AddSingleton<IChangellyService, ChangellyService>();
            services.AddSingleton<IBitcoinService, BitcoinService>();
            services.AddSingleton<IBitGoService, BitGoService>();
            services.AddSingleton<IWavesCoinomatService, WavesCoinomatService>();


            services.AddSingleton<ILookupClient, LookupClient>((x) => new LookupClient()
            {
                Timeout = TimeSpan.FromSeconds(5)
            });

            services.TryAddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddTransient<IIpService, IpService>();
            services.TryAddTransient<IIpAddressParser, CustomRemoteIpRateLimitParser>();
        }

        public static void AddAuthenticationCustom(this IServiceCollection services, IConfiguration configuration, ISettingsService settingsService)
        {
            var secretKey = settingsService.JwtSecret;
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var jwtAppSettingOptions = configuration.GetSection(nameof(ServiceJwtIssuerOptions));

            services.Configure<ServiceJwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(ServiceJwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(ServiceJwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            });

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(ServiceJwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(ServiceJwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(ServiceJwtIssuerOptions.Issuer)];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.SaveToken = true;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiUser", policy =>
                    policy.RequireClaim(
                        ServiceConstants.Strings.JwtClaimIdentifiers.Rol,
                        ServiceConstants.Strings.JwtClaims.ApiAccess));
            });


            services.AddIdentity<DatabaseUser, IdentityRole>()
        .AddEntityFrameworkStores<TokenaireContext>()
        .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // System requires confirmed email
                // before we let him to log in successfully.
                options.SignIn.RequireConfirmedEmail = true;

                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
            });


        }
    }
}

