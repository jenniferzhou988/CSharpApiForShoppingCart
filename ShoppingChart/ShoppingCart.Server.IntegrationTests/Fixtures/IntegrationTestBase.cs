using Microsoft.Extensions.DependencyInjection;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Models;
using Xunit;

namespace ShoppingCart.Server.IntegrationTests.Fixtures;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory Factory;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ShoppingCartContext>();
        await db.Database.EnsureCreatedAsync();

        // Seed a customer for cart creation tests
        if (!db.Customers.Any())
        {
            db.Customers.Add(new Customer
            {
                Id = 1,
                FirstName = "Test",
                LastName = "Customer",
                Email = "test@example.com",
                Status = 1,
                Created = DateTime.UtcNow,
                CreatedBy = "Seed"
            });
        }

        // Seed a product for add-item tests
        if (!db.Products.Any())
        {
            db.Products.Add(new Product
            {
                Id = 1,
                ProductName = "Test Widget",
                Price = 25.00m,
                Status = 1,
                Created = DateTime.UtcNow,
                CreatedBy = "Seed"
            });
        }

        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}