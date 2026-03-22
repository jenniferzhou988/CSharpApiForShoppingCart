using ShoppingCartAPI.Repository.Interface;
using ShoppingCartAPI.Models;
using ShoppingCartAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ShoppingCartAPI.Repository.Repositories
{
    public class UserInfoRepository : GDCTRepository<User>, IUserInfoRepository
    {
        public UserInfoRepository(IDbContextFactory<ShoppingCartContext> dbcontextfactory, IAppLogger<User> logger) : base(dbcontextfactory,logger)
        {
        }

        public async Task<User> GetUserInfoByEmailAsync(string userEmail)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                return await ctx.Set<User>()
                .Where(x => x.Email == userEmail)
                .FirstOrDefaultAsync();
            }
        }
        public async Task<User?> GetUserInfoByUserNameAsync(string userName)
        {
            using (var ctx = _dbcontextfactory.CreateDbContext())
            {
                return await ctx.Set<User>()
                .Where(x => x.UserName == userName)
                .FirstOrDefaultAsync();
            }
        
        }
        /// <summary>
        /// Retrieves a list of users associated with a specific organization ID using a stored procedure.
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        /// <exception cref="RepositoryException"></exception>
        public async Task<IEnumerable<User>> GetUserListByOrgIdAsync(int orgId)
        {
            var usersinOrg = new List<User>();

            try
            {
                if (orgId > 0)
                {
                    using (var ctx = _dbcontextfactory.CreateDbContext())
                    {
                        usersinOrg = await ctx.Users
                                     .FromSqlInterpolated($"EXEC sp_GetUsersByOrgId @OrgId={orgId}")
                                     .ToListAsync();
                    }
                }

                return usersinOrg;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user list by SectorId: {OrgId}", orgId);
                throw new RepositoryException($"An unexpected error occurred while fetching user list by OrgID {orgId}.", ex);
            }
        }

        /// <summary>
        /// Checks if an email is unique within the userinfo context.
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        /// <exception cref="RepositoryException"></exception>
        public async Task<bool> IsEmailUniqueAsync(string userEmail, int Id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userEmail))
                {
                    return false;
                }

                using (var ctx = _dbcontextfactory.CreateDbContext())
                {
                    var exists = await ctx.Users
                        .AnyAsync(u => u.Email == userEmail && u.Id != Id);

                    // If any record exists with the same email and a different Id, it's not unique
                    return !exists;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking email uniqueness: {userEmail}", userEmail);
                throw new RepositoryException($"An unexpected error occurred while checking email uniqueness for {userEmail}.", ex);
            }
        }
    }
}
