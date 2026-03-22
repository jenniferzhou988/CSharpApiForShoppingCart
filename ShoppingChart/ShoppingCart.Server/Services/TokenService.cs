using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShoppingCartAPI.Contracts;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Models;
using ShoppingCartAPI.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShoppingCartAPI.Services
{


  /*  public class JwtOptions
    {
        public string Key { get; set; } = default!;
        public string Issuer { get; set; } = "JwtAuthApi";
        public string Audience { get; set; } = "JwtAuthApi.Client";
        public int AccessTokenTTLMinutes { get; set; } = 15;
        public int RefreshTokenTTLDays { get; set; } = 7;
    }
  */



    public class TokenService : ITokenService
    {

        private readonly ShoppingCartContext _db;
        private readonly JwtOptions _opt;

        public TokenService(ShoppingCartContext db, IOptions<JwtOptions> opt)
        {
            _db = db;
            _opt = opt.Value;
        }



        public (string token, DateTime expiresUtc) CreateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email??string.Empty),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Email??string.Empty),
                new("FullName",user.FullName??string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expireAt = DateTime.UtcNow.AddMinutes(_opt.AccessTokenMinutes);
            foreach(var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role?.RoleName??""));
            }
            var token = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                expires: expireAt,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expireAt);
        }

        public async Task<(string accessToken, string refreshToken, DateTime expiry)> IssueTokenPairAsync(User user, CancellationToken ct = default)
        {
            var (access, expiry) = CreateAccessToken(user);
            var refreshPlain = Crypto.GenerateSecureRandomToken(64);
            var refreshHash = Crypto.Sha256(refreshPlain);



            var expireAt = DateTime.UtcNow.AddDays(_opt.RefreshTokenDays);
            var rec = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refreshHash,
                ExpiresAt = expireAt
            };

            _db.RefreshTokens.Add(rec);
            await _db.SaveChangesAsync(ct);

            return (access, refreshPlain, expireAt);
        }

        public async Task<AuthResponse> RotateRefreshTokenAsync(string presentedRefreshToken, CancellationToken ct = default)
        {
            var hash = Crypto.Sha256(presentedRefreshToken);
            var record = await _db.RefreshTokens.Include(r => r.User)
                .FirstOrDefaultAsync(r => r.TokenHash == hash, ct);

            if (record is null || record.Revoked || record.ExpiresAt <= DateTimeOffset.UtcNow)
                throw new SecurityTokenException("Invalid or expired refresh token");

            // Revoke old
            record.Revoked = true;
            record.RevokedAt = DateTimeOffset.UtcNow;
            record.RevokedReason = "rotated";

            // Issue new pair
            var (access,expiry) = CreateAccessToken(record.User);
            var newRtPlain = Crypto.GenerateSecureRandomToken(64);
            var newRtHash = Crypto.Sha256(newRtPlain);

            var newRec = new RefreshToken
            {
                UserId = record.UserId,
                TokenHash = newRtHash,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(_opt.RefreshTokenDays)
            };

            _db.RefreshTokens.Add(newRec);
            record.ReplacedByTokenId = newRec.Id;

            await _db.SaveChangesAsync(ct);


            return ( new AuthResponse(record.User.Id, record.User.Email, access, newRtPlain,expiry));
            //return (access, newRtPlain);
        }

        public async Task RevokeRefreshTokenAsync(string presentedRefreshToken, string reason = "logout", CancellationToken ct = default)
        {
            var hash = Crypto.Sha256(presentedRefreshToken);
            var record = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash, ct);
            if (record is null || record.Revoked) return;

            record.Revoked = true;
            record.RevokedAt = DateTimeOffset.UtcNow;
            record.RevokedReason = reason;
            await _db.SaveChangesAsync(ct);
        }

        public async Task RevokeAllForUserAsync(int userId, CancellationToken ct = default)
        {
            await _db.RefreshTokens
                .Where(r => r.UserId == userId && !r.Revoked)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.Revoked, true)
                    .SetProperty(r => r.RevokedAt, DateTimeOffset.UtcNow)
                    .SetProperty(r => r.RevokedReason, "logout_all"), ct);
        }






    }
}
