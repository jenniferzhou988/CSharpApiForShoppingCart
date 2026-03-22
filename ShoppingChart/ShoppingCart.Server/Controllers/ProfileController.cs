using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Models;
using System.Security.Claims;

namespace ShoppingCartAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController: ControllerBase
    {
        private readonly ShoppingCartContext _db;
        public ProfileController(ShoppingCartContext db)
        {
            _db = db;
        }
        
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("NameId");

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var id = int.Parse(userId);
            User? user = _db.Users.AsNoTracking().Where(u => u.Id == id)
                .Select(static u => new User { Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = u.FullName,
                    Created = u.Created
                }).FirstOrDefault();
                //.Select(u => new { u.Id, u.Email, u.FirstName, u.LastName, u.FullName,  u.Created })
                //.SingleOrDefaultAsync();

            return (user is null) ? NotFound() : Ok(user);
        }

    }
}
