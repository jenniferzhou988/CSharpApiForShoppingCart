using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Models;
using ShoppingCartAPI.Repository.Interface;

namespace ShoppingCartAPI.Repository.Repositories
{
    public class ProductRepository : GDCTRepository<Product>,IProductRepository
    {
       // private readonly ApplicationDbContext _db;

        public ProductRepository(IDbContextFactory<ShoppingCartContext> dbcontextfactory, IAppLogger<Product> logger) : base(dbcontextfactory, logger)
        {
         //   _db = db;
        }

        public async Task<Product> CreateAsync(Product product)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                ctx.Products.Add(product);
                await ctx.SaveChangesAsync();
                return product;
            }
            
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                return await ctx.Products
                    .Include(p => p.Images)
                    .Include(p => p.ProductCategoryLinks)
                        .ThenInclude(l => l.ProductCategory)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
                
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                return await ctx.Products
                    .Include(p => p.Images)
                    .Include(p => p.ProductCategoryLinks)
                        .ThenInclude(l => l.ProductCategory)
                    .AsNoTracking()
                    .ToListAsync();
            }
             
        }

        public async Task<Product?> UpdateAsync(Product product)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var existing = await ctx.Products.FindAsync(product.Id);
                if (existing == null)
                    return null;

                existing.ProductName = product.ProductName;
                existing.Price = product.Price;
                existing.Description = product.Description;
                // Modified and ModifiedBy are handled by ApplicationDbContext.ApplyAuditInformation/save pipeline
                ctx.Products.Update(existing);
                await ctx.SaveChangesAsync();
                return existing;
            }
                
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var existing = await ctx.Products.FindAsync(id);
                if (existing == null)
                    return false;

                ctx.Products.Remove(existing);
                await ctx.SaveChangesAsync();
                return true;

            }
                
        }

        public async Task<ProductImage?> AddImageAsync(int productId, ProductImage image)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var product = await ctx.Products.FindAsync(productId);
                if (product == null)
                    return null;

                image.ProductId = productId;
                ctx.ProductImages.Add(image);
                await ctx.SaveChangesAsync();
                return image;
            }
        }

        public async Task<bool> RemoveImageAsync(int productId, int imageId)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var image = await ctx.ProductImages
                    .FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == productId);
                if (image == null)
                    return false;

                ctx.ProductImages.Remove(image);
                await ctx.SaveChangesAsync();
                return true;
            }
        }

        public async Task<ProductCategoryLink?> AddCategoryAsync(int productId, int categoryId)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var product = await ctx.Products.FindAsync(productId);
                if (product == null)
                    return null;

                var category = await ctx.ProductCategories.FindAsync(categoryId);
                if (category == null)
                    return null;

                var existingLink = await ctx.ProductCategoryLinks
                    .FirstOrDefaultAsync(l => l.ProductId == productId && l.ProductCategoryId == categoryId);
                if (existingLink != null)
                    return existingLink;

                var link = new ProductCategoryLink
                {
                    ProductId = productId,
                    ProductCategoryId = categoryId
                };

                ctx.ProductCategoryLinks.Add(link);
                await ctx.SaveChangesAsync();
                return link;
            }
        }

        public async Task<bool> RemoveCategoryAsync(int productId, int categoryId)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var link = await ctx.ProductCategoryLinks
                    .FirstOrDefaultAsync(l => l.ProductId == productId && l.ProductCategoryId == categoryId);
                if (link == null)
                    return false;

                ctx.ProductCategoryLinks.Remove(link);
                await ctx.SaveChangesAsync();
                return true;
            }
        }

        public async Task<ProductImportRecord?> ImportProductAsync(int productId, decimal importPrice, int quantity, string? comment)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                var product = await ctx.Products.FindAsync(productId);
                if (product == null)
                    return null;

                // Create ProductImportRecord
                var importRecord = new ProductImportRecord
                {
                    ProductId = productId,
                    ImportPrice = importPrice,
                    Quantity = quantity,
                    Comment = comment,
                    Status = 1,
                    Created = DateTime.UtcNow
                };

                ctx.ProductImportRecords.Add(importRecord);

                // Create ProductInventory if not exist, otherwise update Quantity
                var inventory = await ctx.ProductInventories
                    .FirstOrDefaultAsync(i => i.ProductId == productId);

                if (inventory == null)
                {
                    inventory = new ProductInventory
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        Description = $"Inventory for {product.ProductName}",
                        Comment = $"Created from import on {DateTime.UtcNow:yyyy-MM-dd}",
                        Status = 1,
                        Created = DateTime.UtcNow
                    };
                    ctx.ProductInventories.Add(inventory);
                }
                else
                {
                    inventory.Quantity += quantity;
                    inventory.Modified = DateTime.UtcNow;
                    ctx.ProductInventories.Update(inventory);
                }

                await ctx.SaveChangesAsync();
                return importRecord;
            }
        }
    }
}