using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Contracts;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Models;
using ShoppingCartAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ShoppingCartAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly ShoppingCartContext _db;
        private readonly ITokenService _tokens;
        private readonly IConfiguration _cfg;

        private const string RtCookie = "rt";

        public AuthController(ShoppingCartContext db, ITokenService tokens, IConfiguration cfg)
        {
            _db = db;
            _tokens = tokens;
            _cfg = cfg;
        }


        private void SetRefreshCookie(string refreshToken, int days)
        {
            var isProd = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
            Response.Cookies.Append(RtCookie, refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = isProd, // true under HTTPS in prod
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(days)
            });
        }


        private void ClearRefreshCookie() =>
               Response.Cookies.Delete(RtCookie, new CookieOptions { Path = "/" });


        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Email and password are required.");

            if (req.Password != req.ConfirmPassword)
                return BadRequest("Passwords do not match.");

            var exists = await _db.Users.AnyAsync(u => u.Email == req.Email, ct);
            if (exists) return Conflict("Email already registered.");

            // need to modify the code and apply detail user information here
            var user = new User
            {
                Email = req.Email.Trim().ToLowerInvariant(),
                UserName = req.Email.Trim().ToLowerInvariant(),
                FullName = $"{req.FirstName} {req.LastName}".Trim(),
                FirstName = req.FirstName,
                LastName = req.LastName,
                Status = 1,
                Created = DateTime.UtcNow,
                CreatedBy = "System",
                UserRoles = new List<UserRole>()
            };
            user.UserRoles.Add(new UserRole
            {
                RoleId = 2, // Default to USER role
                Status = 1,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password, workFactor: 12);

            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);

            // Assign default Role as USER (Id = 2)
            var userRole = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "USER", ct);
            if (userRole != null)
            {
                var userRoleLink = new UserRole
                {
                    UserId = user.Id,
                    RoleId = userRole.Id,
                    Status = 1,
                    Created = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                _db.UserRoles.Add(userRoleLink);
            }

            // Create a Customer record linked to the User
            var customer = new Customer
            {
                FirstName = req.FirstName,
                MiddleName = req.MiddleName,
                LastName = req.LastName,
                Email = req.Email.Trim().ToLowerInvariant(),
                Status = 1,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            };
            _db.Customers.Add(customer);
            await _db.SaveChangesAsync(ct);

            // Link User to Customer
            var userCustomerLink = new UserCustomerLink
            {
                UserId = user.Id,
                CustomerId = customer.Id,
                Status = 1,
                Created = DateTime.UtcNow,
                CreatedBy = "System"
            };
            _db.UserCustomerLinks.Add(userCustomerLink);
            await _db.SaveChangesAsync(ct);

            var (access, refresh, expiry) = await _tokens.IssueTokenPairAsync(user, ct);
            if (_cfg.GetValue<bool>("UseCookiesForRefreshToken"))
            {
                var ttlDays = _cfg.GetValue<int>("Jwt:RefreshTokenDays");
                SetRefreshCookie(refresh, ttlDays);
            }

            return CreatedAtAction(nameof(Register), new AuthResponse(user.Id, user.Email, access, refresh, expiry));
        }

        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
        {

            var user = await _db.Users.AsNoTracking()
                .Select(static o => new User()
            {
                Id = o.Id,
                Email = o.Email,
                FullName = o.FullName,
                FirstName = o.FirstName,
                LastName = o.LastName,
                PasswordHash = o.PasswordHash,
                Status = 1,
                UserRoles =o.UserRoles,
                Roles=o.UserRoles.Select(ur=>ur.Role).ToList()
                })
            .SingleOrDefaultAsync(u =>
            u.Email == req.Email.Trim().ToLowerInvariant()
            ,ct);

            if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                return Unauthorized(new ErrorResponse("Invalid credentials"));

            var (access, refresh,expiry) = await _tokens.IssueTokenPairAsync(user, ct);

            if (_cfg.GetValue<bool>("UseCookiesForRefreshToken"))
            {
                var ttlDays = _cfg.GetValue<int>("Jwt:RefreshTokenDays");
                SetRefreshCookie(refresh, ttlDays);
            }

            return Ok(new AuthResponse(user.Id, user.Email, access, refresh,expiry));

            //return CreatedAtAction(nameof(Register), new AuthResponse(user.Id, user.Email, access,refresh, user));
        }



        public record RefreshRequest(string? RefreshToken);

        [HttpPost("refresh")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
        {
            var useCookie = _cfg.GetValue<bool>("UseCookiesForRefreshToken");
            var presented = useCookie ? Request.Cookies[RtCookie] : req.RefreshToken;

            if (string.IsNullOrWhiteSpace(presented))
                return Unauthorized(new ErrorResponse("Missing refresh token"));

            try
            {
                var auth= await _tokens.RotateRefreshTokenAsync(presented!, ct);

                if (useCookie)
                {
                    var ttlDays = _cfg.GetValue<int>("Jwt:RefreshTokenDays");
                    SetRefreshCookie(auth.RefreshToken, ttlDays);
                    return Ok(auth);
                }

                return Ok(auth);
            }
            catch
            {
                return Unauthorized(new ErrorResponse("Refresh failed"));
            }
        }

        [HttpPost("logout")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest req, CancellationToken ct)
        {
            var useCookie = _cfg.GetValue<bool>("UseCookiesForRefreshToken");
            var presented = useCookie ? Request.Cookies[RtCookie] : req.RefreshToken;

            if (!string.IsNullOrWhiteSpace(presented))
            {
                await _tokens.RevokeRefreshTokenAsync(presented!, "logout", ct);
            }

            if (useCookie) ClearRefreshCookie();
            return Ok(new SuccessResponse());
        }

        [HttpPost("logout-all")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> LogoutAll(CancellationToken ct)
        {
            // Get user id from access token claims (Authorization: Bearer ...)
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? User.FindFirstValue(JwtRegisteredClaimNames.NameId);

            if (int.TryParse(userIdStr, out var userId))
            {
                await _tokens.RevokeAllForUserAsync(userId, ct);
            }

            ClearRefreshCookie();
            return Ok(new SuccessResponse());
        }





    }
}
