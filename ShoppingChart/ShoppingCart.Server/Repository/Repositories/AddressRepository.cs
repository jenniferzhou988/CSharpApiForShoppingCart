using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Models;
using ShoppingCartAPI.Repository.Interface;

namespace ShoppingCartAPI.Repository.Repositories
{
    public class AddressRepository : GDCTRepository<Address>, IAddressRepository
    {
        public AddressRepository(IDbContextFactory<ShoppingCartContext> dbcontextfactory, IAppLogger<Address> logger) : base(dbcontextfactory, logger)
        {
        }

        public async Task<Address> CreateAsync(Address address)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                ctx.Addresses.Add(address);
                await ctx.SaveChangesAsync();
                return address;
            }
        }

        public async Task<Address?> GetByIdAsync(int id)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                return await ctx.Addresses.FindAsync(id);
            }
        }

        public async Task<IEnumerable<Address>> GetAllAsync()
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                return await ctx.Addresses.AsNoTracking().ToListAsync();
            }
        }

        public async Task<Address?> UpdateAsync(Address address)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var existing = await ctx.Addresses.FindAsync(address.Id);
                if (existing == null)
                    return null;

                existing.StreetNo = address.StreetNo;
                existing.Street = address.Street;
                existing.City = address.City;
                existing.PostalCode = address.PostalCode;
                existing.Province = address.Province;
                existing.Country = address.Country;

                ctx.Addresses.Update(existing);
                await ctx.SaveChangesAsync();
                return existing;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var existing = await ctx.Addresses.FindAsync(id);
                if (existing == null)
                    return false;

                ctx.Addresses.Remove(existing);
                await ctx.SaveChangesAsync();
                return true;
            }
        }
    }
}