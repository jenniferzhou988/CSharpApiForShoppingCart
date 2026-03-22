using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Models;
using ShoppingCartAPI.Repository.Interface;

namespace ShoppingCartAPI.Repository.Repositories
{
    public class ShippingTrackingRepository : GDCTRepository<ShippingTracking>, IShippingTrackingRepository
    {
        public ShippingTrackingRepository(IDbContextFactory<ShoppingCartContext> dbcontextfactory, IAppLogger<ShippingTracking> logger) : base(dbcontextfactory, logger)
        {
        }

        public async Task<ShippingTracking?> CreateShippingTrackingAsync(CreateShippingTrackingRequest request)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                // Validate Order
                var order = await ctx.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId);
                if (order == null)
                    return null;

                // Check if ShippingServiceProvider exists by name, create if not
                var provider = await ctx.ShippingServiceProviders
                    .FirstOrDefaultAsync(p => p.ProviderName == request.ProviderName);

                if (provider == null)
                {
                    provider = new ShippingServiceProvider
                    {
                        ProviderName = request.ProviderName,
                        Status = 1,
                        Created = DateTime.UtcNow
                    };
                    ctx.ShippingServiceProviders.Add(provider);
                    await ctx.SaveChangesAsync();
                }

                // Create ShippingTracking
                var tracking = new ShippingTracking
                {
                    ShippingServiceProviderId = provider.Id,
                    TrackingNumber = request.TrackingNumber,
                    ShippingDate = request.ShippingDate,
                    Comment = request.Comment,
                    Status = 1,
                    Created = DateTime.UtcNow
                };

                ctx.ShippingTrackings.Add(tracking);
                await ctx.SaveChangesAsync();

                // Create ShippingItemDetail for each OrderDetailId
                foreach (var orderDetailId in request.OrderDetailIds)
                {
                    var orderDetail = order.OrderDetails.FirstOrDefault(d => d.Id == orderDetailId);
                    if (orderDetail == null)
                        continue;

                    var shippingItem = new ShippingItemDetail
                    {
                        ShippingTrackingId = tracking.Id,
                        OrderDetailId = orderDetailId,
                        Status = 1,
                        Created = DateTime.UtcNow
                    };

                    ctx.ShippingItemDetails.Add(shippingItem);
                }

                // Update Order status to "Shipped"
                var shippedStatus = await ctx.OrderStatuses
                    .FirstOrDefaultAsync(s => s.OrderStatusName == "Shipped");
                if (shippedStatus != null)
                {
                    order.OrderStatusId = shippedStatus.Id;
                    order.Modified = DateTime.UtcNow;
                    ctx.Orders.Update(order);
                }

                await ctx.SaveChangesAsync();

                // Return the full tracking with details
                return await ctx.ShippingTrackings
                    .Include(t => t.ShippingServiceProvider)
                    .Include(t => t.ShippingItemDetails)
                        .ThenInclude(i => i.OrderDetail)
                            .ThenInclude(d => d.Product)
                    .FirstOrDefaultAsync(t => t.Id == tracking.Id);
            }
        }

        public new async Task<ShippingTracking?> GetByIdAsync(int id)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                return await ctx.ShippingTrackings
                    .Include(t => t.ShippingServiceProvider)
                    .Include(t => t.ShippingItemDetails)
                        .ThenInclude(i => i.OrderDetail)
                            .ThenInclude(d => d.Product)
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
        }

        public new async Task<IEnumerable<ShippingTracking>> GetAllAsync()
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                return await ctx.ShippingTrackings
                    .Include(t => t.ShippingServiceProvider)
                    .Include(t => t.ShippingItemDetails)
                        .ThenInclude(i => i.OrderDetail)
                            .ThenInclude(d => d.Product)
                    .AsNoTracking()
                    .ToListAsync();
            }
        }
    }
}