using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Models;

namespace ShoppingCartAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductCategoryController : ControllerBase
    {
        private readonly ShoppingCartContext _db;

        public ProductCategoryController(ShoppingCartContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _db.ProductCategories
                .Where(c => c.Status == 1)
                .OrderBy(c => c.CategoryName)
                .Select(c => new
                {
                    c.Id,
                    c.CategoryName,
                    c.Description,
                    ProductCount = c.ProductCategoryLinks.Count
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(categories);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _db.ProductCategories
                .Where(c => c.Id == id && c.Status == 1)
                .Select(c => new
                {
                    c.Id,
                    c.CategoryName,
                    c.Description,
                    ProductCount = c.ProductCategoryLinks.Count
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (category == null) return NotFound();
            return Ok(category);
        }

        [HttpGet("{id:int}/products")]
        public async Task<IActionResult> GetProductsByCategory(int id)
        {
            var category = await _db.ProductCategories.FindAsync(id);
            if (category == null) return NotFound();

            var products = await _db.ProductCategoryLinks
                .Where(l => l.ProductCategoryId == id && l.Product.Status == 1)
                .Include(l => l.Product)
                    .ThenInclude(p => p.Images)
                .Include(l => l.Product)
                    .ThenInclude(p => p.ProductInventories)
                .Select(l => l.Product)
                .AsNoTracking()
                .ToListAsync();

            return Ok(products);
        }
    }
}