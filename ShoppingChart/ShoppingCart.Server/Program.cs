using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Models;
using ShoppingCartAPI.Repository.Interface;
using ShoppingCartAPI.Repository.Repositories;
using ShoppingCartAPI.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1) Configuration binding
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// 2) Encryption service (required by ShoppingCartContext)
builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();

// 3) EF Core — use AddDbContextFactory so IDbContextFactory<> is available for repositories
builder.Services.AddDbContextFactory<ShoppingCartContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3) AuthN: JWT Bearer
var jwtSection = builder.Configuration.GetSection("Jwt");
var signingKey = jwtSection["SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey missing");
var issuer = jwtSection["Issuer"] ?? "DataCollectionApi";
var audience = jwtSection["Audience"] ?? "DataCollectionClients";


builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,

            ValidateAudience = true,
            ValidAudience = audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
// Authorization
builder.Services.AddAuthorization();

// 4) AuthZ
/*builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.AddPolicy("AdminsOnly", p => p.RequireRole("Admin"));
}); */

// 5) Utilities
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
//builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IShippingTrackingRepository, ShippingTrackingRepository>();

// 1) Add CORS
builder.Services.AddCors(opts =>
{
    /*

   opts.AddDefaultPolicy(policy =>
   {
       var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
       policy.WithOrigins(origins)
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials();
   });
    */
   opts.AddPolicy("DevCors", policy =>
   {
       policy
           .WithOrigins(
               "https://127.0.0.1:53109", // Angular dev origin (match yours)
               "https://localhost:53109",  // optionally add localhost if you use it
               "http://localhost:4200"
           )
           .AllowAnyHeader()
           .AllowAnyMethod()
           ;
   });
});


// Minimal rate limiting for auth endpoints
builder.Services.AddRateLimiter(options =>
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


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// 6) Swagger (with Bearer)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter **Bearer &lt;token&gt;**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});


var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("DevCors");
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();


app.MapFallbackToFile("/index.html");


// Simple health check
app.MapGet("/health", () => Results.Ok(new { ok = true }));

app.Run();

// Expose entry point for integration tests (WebApplicationFactory<Program>)
public partial class Program { }
