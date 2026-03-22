using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Models;
using ShoppingCartAPI.Repository.Interface;

namespace ShoppingCartAPI.Repository.Repositories
{
    public class OrderRepository : GDCTRepository<Order>, IOrderRepository
    {
        public OrderRepository(IDbContextFactory<ShoppingCartContext> dbcontextfactory, IAppLogger<Order> logger) : base(dbcontextfactory, logger)
        {
        }

        public async Task<Order?> CreateOrderAsync(int customerId, int bankCardId, int shippingAddressId, int billingAddressId, List<CreateOrderItemRequest> items)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                // Validate Customer
                var customer = await ctx.Customers.FindAsync(customerId);
                if (customer == null)
                    return null;

                // Validate BankCardInfo
                var bankCard = await ctx.BankCardInfos.FindAsync(bankCardId);
                if (bankCard == null)
                    return null;

                // Validate Shipping Address
                var shippingAddress = await ctx.Addresses.FindAsync(shippingAddressId);
                if (shippingAddress == null)
                    return null;

                // Validate Billing Address
                var billingAddress = await ctx.Addresses.FindAsync(billingAddressId);
                if (billingAddress == null)
                    return null;

                // OrderStatus "Ordered" has Id = 1
                var orderedStatus = await ctx.OrderStatuses
                    .FirstOrDefaultAsync(s => s.OrderStatusName == "Ordered");
                if (orderedStatus == null)
                    return null;

                // Create Order
                var order = new Order
                {
                    CustomerId = customerId,
                    BankCardId = bankCardId,
                    ShippingAddressId = shippingAddressId,
                    BillingAddressId = billingAddressId,
                    OrderStatusId = orderedStatus.Id,
                    Status = 1,
                    Created = DateTime.UtcNow
                };

                ctx.Orders.Add(order);
                await ctx.SaveChangesAsync();

                // Create OrderDetail items and update ProductInventory
                foreach (var item in items)
                {
                    var product = await ctx.Products.FindAsync(item.ProductId);
                    if (product == null)
                        continue;

                    var detail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Price = product.Price,
                        Quantity = item.Quantity,
                        TotalPrice = product.Price * item.Quantity,
                        Comment = item.Comment,
                        Status = 1,
                        Created = DateTime.UtcNow
                    };

                    ctx.OrderDetails.Add(detail);

                    // Deduct quantity from ProductInventory
                    var inventory = await ctx.ProductInventories
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                    if (inventory != null)
                    {
                        inventory.Quantity -= item.Quantity;
                        inventory.Modified = DateTime.UtcNow;
                        ctx.ProductInventories.Update(inventory);
                    }
                }

                await ctx.SaveChangesAsync();

                // Return the full order with details
                return await ctx.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.BankCardInfo)
                    .Include(o => o.ShippingAddress)
                    .Include(o => o.BillingAddress)
                    .Include(o => o.OrderStatus)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(d => d.Product)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);
            }
        }

        public new async Task<Order?> GetByIdAsync(int id)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                return await ctx.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.BankCardInfo)
                    .Include(o => o.ShippingAddress)
                    .Include(o => o.BillingAddress)
                    .Include(o => o.OrderStatus)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(d => d.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);
            }
        }

        public new async Task<IEnumerable<Order>> GetAllAsync()
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                return await ctx.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.BankCardInfo)
                    .Include(o => o.ShippingAddress)
                    .Include(o => o.BillingAddress)
                    .Include(o => o.OrderStatus)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(d => d.Product)
                    .AsNoTracking()
                    .ToListAsync();
            }
        }
    }
}