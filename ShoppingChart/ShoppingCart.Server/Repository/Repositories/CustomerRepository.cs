using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Models;
using ShoppingCartAPI.Repository.Interface;

namespace ShoppingCartAPI.Repository.Repositories
{
    public class CustomerRepository : GDCTRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(IDbContextFactory<ShoppingCartContext> dbcontextfactory, IAppLogger<Customer> logger) : base(dbcontextfactory, logger)
        {
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                ctx.Customers.Add(customer);
                await ctx.SaveChangesAsync();
                return customer;
            }
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                return await ctx.Customers
                    .Include(c => c.CustomerAddressLinks)
                        .ThenInclude(l => l.Address)
                    .Include(c => c.CustomerAddressLinks)
                        .ThenInclude(l => l.AddressType)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                return await ctx.Customers
                    .Include(c => c.CustomerAddressLinks)
                        .ThenInclude(l => l.Address)
                    .Include(c => c.CustomerAddressLinks)
                        .ThenInclude(l => l.AddressType)
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        public async Task<Customer?> UpdateAsync(Customer customer)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var existing = await ctx.Customers.FindAsync(customer.Id);
                if (existing == null)
                    return null;

                existing.FirstName = customer.FirstName;
                existing.MiddleName = customer.MiddleName;
                existing.LastName = customer.LastName;
                existing.Email = customer.Email;
                existing.PhoneNumber = customer.PhoneNumber;

                ctx.Customers.Update(existing);
                await ctx.SaveChangesAsync();
                return existing;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var existing = await ctx.Customers.FindAsync(id);
                if (existing == null)
                    return false;

                ctx.Customers.Remove(existing);
                await ctx.SaveChangesAsync();
                return true;
            }
        }

        public async Task<CustomerAddressLink?> AddAddressAsync(int customerId, Address address, int addressTypeId)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var customer = await ctx.Customers.FindAsync(customerId);
                if (customer == null)
                    return null;

                var addressType = await ctx.AddressTypes.FindAsync(addressTypeId);
                if (addressType == null)
                    return null;

                ctx.Addresses.Add(address);
                await ctx.SaveChangesAsync();

                var link = new CustomerAddressLink
                {
                    CustomerId = customerId,
                    AddressId = address.Id,
                    AddressTypeId = addressTypeId
                };

                ctx.CustomerAddressLinks.Add(link);
                await ctx.SaveChangesAsync();
                return link;
            }
        }

        public async Task<CustomerAddressLink?> UpdateAddressAsync(int customerId, int addressLinkId, Address address, int addressTypeId)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var link = await ctx.CustomerAddressLinks
                    .Include(l => l.Address)
                    .FirstOrDefaultAsync(l => l.Id == addressLinkId && l.CustomerId == customerId);
                if (link == null)
                    return null;

                var addressType = await ctx.AddressTypes.FindAsync(addressTypeId);
                if (addressType == null)
                    return null;

                link.Address.StreetNo = address.StreetNo;
                link.Address.Street = address.Street;
                link.Address.City = address.City;
                link.Address.PostalCode = address.PostalCode;
                link.Address.Province = address.Province;
                link.Address.Country = address.Country;
                link.AddressTypeId = addressTypeId;

                ctx.CustomerAddressLinks.Update(link);
                await ctx.SaveChangesAsync();
                return link;
            }
        }

        public async Task<bool> RemoveAddressAsync(int customerId, int addressLinkId)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var link = await ctx.CustomerAddressLinks
                    .Include(l => l.Address)
                    .FirstOrDefaultAsync(l => l.Id == addressLinkId && l.CustomerId == customerId);
                if (link == null)
                    return false;

                ctx.Addresses.Remove(link.Address);
                ctx.CustomerAddressLinks.Remove(link);
                await ctx.SaveChangesAsync();
                return true;
            }
        }
    }
}