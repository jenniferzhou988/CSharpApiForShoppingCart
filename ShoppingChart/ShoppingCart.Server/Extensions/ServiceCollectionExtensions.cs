using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Services;
using System.Text;

namespace ShoppingCartAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {


        public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
        {
            // EF Core - demo
           // services.AddDbContext<GdctContext>(o => o.UseInMemoryDatabase("AuthDb"));
            // For SQL Server:
            services.AddDbContext<ShoppingCartContext>(o => o.UseSqlServer(config.GetConnectionString("GDCTConnection")));

            services.Configure<JwtOptions>(config.GetSection("Jwt"));
            services.AddScoped<ITokenService, TokenService>();
            services.AddSingleton<IEncryptionService, AesEncryptionService>();

            // JWT Bearer
            var jwt = config.GetSection("Jwt").Get<JwtOptions>()!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.RequireHttpsMetadata = false; // set true in production with HTTPS
                opt.SaveToken = true;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = key
                };
            });

            services.AddAuthorization();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                    policy.WithOrigins(origins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // Minimal rate limiting for auth endpoints
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter(policyName: "auth",
                    configureOptions: _ => {
                        _.AutoReplenishment = true;
                        _.PermitLimit = 100;
                        _.Window = TimeSpan.FromMinutes(10);
                        _.QueueLimit = 0;
                    });
                options.AddFixedWindowLimiter(policyName: "login",
                    configureOptions: _ => {
                        _.AutoReplenishment = true;
                        _.PermitLimit = 10;
                        _.Window = TimeSpan.FromMinutes(10);
                        _.QueueLimit = 0;
                    });
            });

            services.AddControllers();
            return services;
        }



    }
}
