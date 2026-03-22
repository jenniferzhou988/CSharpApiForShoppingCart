using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Repository.Interface;
using ShoppingCartAPI.Services;

namespace ShoppingCart.Server.IntegrationTests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all EF Core / DbContext registrations
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<ShoppingCartContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(ShoppingCartContext) ||
                    d.ServiceType == typeof(IDbContextFactory<ShoppingCartContext>))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Remove real IEncryptionService and replace with a no-op mock
            var encryptionDescriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(IEncryptionService));
            if (encryptionDescriptor != null)
                services.Remove(encryptionDescriptor);

            var mockEncryption = new Mock<IEncryptionService>();
            mockEncryption.Setup(e => e.Encrypt(It.IsAny<string>())).Returns<string>(s => $"ENC_{s}");
            mockEncryption.Setup(e => e.Decrypt(It.IsAny<string>())).Returns<string>(s => s.StartsWith("ENC_") ? s[4..] : s);
            services.AddSingleton(mockEncryption.Object);

            // Register IAppLogger<T> as no-op so repository construction doesn't fail
            services.AddSingleton(typeof(IAppLogger<>), typeof(NullAppLogger<>));

            // Add in-memory database
            services.AddDbContext<ShoppingCartContext>((sp, options) =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            // Register IDbContextFactory (required by ShoppingCartRepository)
            services.AddDbContextFactory<ShoppingCartContext>((sp, options) =>
            {
                options.UseInMemoryDatabase(_dbName);
            }, ServiceLifetime.Scoped);
        });

        builder.UseEnvironment("Development");
    }

    /// <summary>
    /// No-op logger used in test to satisfy IAppLogger&lt;T&gt; dependencies.
    /// </summary>
    private class NullAppLogger<T> : IAppLogger<T>
    {
        public void LogInformation(string message, params object[] args) { }
        public void LogWarning(string message, params object[] args) { }
        public void LogError(string message, params object[] args) { }
        public void LogError(Exception ex, string message, params object[] args) { }
    }
}