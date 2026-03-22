using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Models;
using ShoppingCartAPI.Repository.Interface;

namespace ShoppingCartAPI.Repository.Repositories
{
    public class ShoppingCartRepository : GDCTRepository<Models.ShoppingCart>, IShoppingCartRepository
    {
        public ShoppingCartRepository(IDbContextFactory<ShoppingCartContext> dbcontextfactory, IAppLogger<Models.ShoppingCart> logger) : base(dbcontextfactory, logger)
        {
        }

        public async Task<Models.ShoppingCart> CreateAsync(int customerId)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var customer = await ctx.Customers.FindAsync(customerId);
                if (customer == null)
                    throw new ArgumentException($"Customer with Id={customerId} not found.");

                var cart = new Models.ShoppingCart
                {
                    CustomerId = customerId,
                    Status = 1,
                    Created = DateTime.UtcNow
                };

                ctx.ShoppingCarts.Add(cart);
                await ctx.SaveChangesAsync();
                return cart;
            }
        }

        public new async Task<Models.ShoppingCart?> GetByIdAsync(int id)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                return await ctx.ShoppingCarts
                    .Include(s => s.Customer)
                    .Include(s => s.ShoppingCartDetails)
                        .ThenInclude(d => d.Product)
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
        }

        public new async Task<IEnumerable<Models.ShoppingCart>> GetAllAsync()
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                return await ctx.ShoppingCarts
                    .Include(s => s.Customer)
                    .Include(s => s.ShoppingCartDetails)
                        .ThenInclude(d => d.Product)
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var cart = await ctx.ShoppingCarts
                    .Include(s => s.ShoppingCartDetails)
                    .FirstOrDefaultAsync(s => s.Id == id);
                if (cart == null)
                    return false;

                ctx.ShoppingCartDetails.RemoveRange(cart.ShoppingCartDetails);
                ctx.ShoppingCarts.Remove(cart);
                await ctx.SaveChangesAsync();
                return true;
            }
        }

        public async Task<ShoppingCartDetail?> AddItemAsync(int shoppingCartId, int productId, int quantity)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var cart = await ctx.ShoppingCarts.FindAsync(shoppingCartId);
                if (cart == null)
                    return null;

                var product = await ctx.Products.FindAsync(productId);
                if (product == null)
                    return null;

                var detail = new ShoppingCartDetail
                {
                    ShoppingCartId = shoppingCartId,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = product.Price,
                    TotalPrice = product.Price * quantity,
                    Status = 1,
                    Created = DateTime.UtcNow
                };

                ctx.ShoppingCartDetails.Add(detail);
                await ctx.SaveChangesAsync();
                return detail;
            }
        }

        public async Task<ShoppingCartDetail?> UpdateItemAsync(int shoppingCartId, int detailId, int quantity)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var detail = await ctx.ShoppingCartDetails
                    .Include(d => d.Product)
                    .FirstOrDefaultAsync(d => d.Id == detailId && d.ShoppingCartId == shoppingCartId);
                if (detail == null)
                    return null;

                detail.Quantity = quantity;
                detail.TotalPrice = detail.Price * quantity;
                detail.Modified = DateTime.UtcNow;

                ctx.ShoppingCartDetails.Update(detail);
                await ctx.SaveChangesAsync();
                return detail;
            }
        }

        public async Task<bool> RemoveItemAsync(int shoppingCartId, int detailId)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var detail = await ctx.ShoppingCartDetails
                    .FirstOrDefaultAsync(d => d.Id == detailId && d.ShoppingCartId == shoppingCartId);
                if (detail == null)
                    return false;

                ctx.ShoppingCartDetails.Remove(detail);
                await ctx.SaveChangesAsync();
                return true;
            }
        }
    }
}