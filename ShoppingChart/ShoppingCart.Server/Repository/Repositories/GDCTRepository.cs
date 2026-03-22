using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Models;
using ShoppingCartAPI.Repository.Interface;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Reflection;
using static Dapper.SqlMapper;


namespace ShoppingCartAPI.Repository.Repositories
{
    /***************************************************************
    * ClassName    : GDCTRepository
    * Created By   : Bill Chen
    * Description  : GDCT base entity 
    * -------------------------------------------------------------
    * History      : 2024-08-22 Initial created
    *              : 2025-02-18 Add AuditLog - Jet Su
    *              : 2025-02-20 Restructure the Repositories classes - Jet Su
    *              : 2025-06-02 Standarlized the coding - Jet Su
    ***************************************************************/
    public class GDCTRepository<T> : IGDCTRepository<T> where T : GDCTEntityBase<int>
    {
        protected readonly IAppLogger<T> _logger;
        protected readonly IDbContextFactory<ShoppingCartContext> _dbcontextfactory;

        public GDCTRepository(IDbContextFactory<ShoppingCartContext> dbcontextfactory, IAppLogger<T> logger)
        {
            _dbcontextfactory = dbcontextfactory;
            _logger = logger;
        }

        public async Task<IEnumerable<T>> GetPageSizeListAsync(int pageNumber, int pageSize)
        {
            IEnumerable<T> result = Enumerable.Empty<T>();
            try
            {
                using (ShoppingCartContext ctx = _dbcontextfactory.CreateDbContext())
                {
                    result =  await ctx.Set<T>()
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw new RepositoryException("$An unexpected error occurred while getting page size for PageNumber={pageNumber} and PageSize={pageSize}", ex);
            }
            return result;
        }

        public async Task<T> AddAsync(T entity)
        {
            try
            {
                using (ShoppingCartContext ctx = _dbcontextfactory.CreateDbContext())
                {
                    ctx.Set<T>().Add(entity);
                    Task.Run(() => ctx.SaveChangesAsync()).GetAwaiter().GetResult();

                    PropertyInfo? IdProperty = entity.GetType().GetProperty("Id");
                    if (IdProperty != null)
                    {
                        var keyValue = entity.GetType().GetProperty("Id")?.GetValue(entity, null);
                        if (keyValue is int intValue)
                        {
                            entity.Id = intValue;
                        }
                    }
                    //Get insert keyvalue
                    Task.Run(() => GetPrimayKeyValue(entity, ctx)).GetAwaiter().GetResult();
                    //make sure it been inserted
                    await SaveAuditLog(entity, ctx, "LogSteps.Insert");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw new RepositoryException("An unexpected error occurred while adding a new item for " + entity.GetType().Name, ex);
            }

            return entity;
        }

        public async Task DeleteAsync(T entity)
        {
            try
            {
                entity.Status = 0;
                using (ShoppingCartContext ctx = _dbcontextfactory.CreateDbContext())
                {
                    Task.Run(() => SaveAuditLog(entity, ctx, "ogSteps.BeforeUpdate")).GetAwaiter().GetResult();

                    ctx.Entry(entity).State = EntityState.Modified;
                    Task.Run(() => ctx.SaveChangesAsync()).GetAwaiter().GetResult();

                    //make sure the data been modified before log after update data
                    await SaveAuditLog(entity, ctx, "LogSteps.AfterUpdate");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw new RepositoryException("An unexpected error occurred while delete an item for " + entity.GetType().Name, ex);
            }
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            List<T> result = new List<T>(); 
            try
            {
                using (ShoppingCartContext ctx = _dbcontextfactory.CreateDbContext())
                {
                    result = await ctx.Set<T>().ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                T entity = Activator.CreateInstance<T>();
                throw new RepositoryException("An unexpected error occurred while getting all items for " + entity.GetType().Name, ex);
            }

            return result;
        }

        public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            List<T> result = new List<T>();
            try
            {
                using (ShoppingCartContext ctx = _dbcontextfactory.CreateDbContext())
                {
                    result =  await ctx.Set<T>().Where(predicate).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                T entity = Activator.CreateInstance<T>();
                throw new RepositoryException("An unexpected error occurred while getting items for " + entity.GetType().Name, ex);
            }
            return result;
        }

        public async Task<T> GetByIdAsync(int id)
        {
            T result = default(T);
            try
            {
                using (ShoppingCartContext ctx = _dbcontextfactory.CreateDbContext())
                {
                    result =  await ctx.Set<T>().FindAsync(id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                T entity = Activator.CreateInstance<T>();
                throw new RepositoryException("$An unexpected error occurred while getting one item by Id={id} for " + entity.GetType().Name, ex);
            }
            return result;
        }

        public async Task UpdateAsync(T entity)
        {
            try
            {
                using (ShoppingCartContext ctx = _dbcontextfactory.CreateDbContext())
                {
                    Task.Run(() => SaveAuditLog(entity, ctx, "LogSteps.BeforeUpdate")).GetAwaiter().GetResult();
                    ctx.Entry(entity).State = EntityState.Modified;
                    Task.Run(() => ctx.SaveChangesAsync()).GetAwaiter().GetResult();

                    //make sure the data been modified before log after update data
                    await SaveAuditLog(entity, ctx, "LogSteps.AfterUpdate");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw new RepositoryException("An unexpected error occurred while updating " + entity.GetType().Name, ex);
            }            
        }

        /***************************************************************
         * Function Name: SaveAuditLog
         * Created By   : Jet Su  
         * Description  : Save general Audit log
         * parameters   : T       - table entity
         *                gdct    - Gdct db context
         *                logStep - DB update step : Insert/Update/Delete
         * -------------------------------------------------------------
         * History : 2025-01-20 Initial created - Jet 
         *         : 2025-02-18 replace hard code with Constant - Jet 
         *         : 2025-03-14 Add logic for key as ID - Jet 
         ***************************************************************/
        private async Task SaveAuditLog(T entity, ShoppingCartContext gdct,string logStep)
        {
            try
            {
                if (!gdct.Database.IsRelational())
                    return;

                string tableName = entity.GetType().Name;
                string idName = tableName + "Id";
                string idValue = entity.Id.ToString();
                string updateBy = string.IsNullOrEmpty(entity.ModifiedBy) ? entity.CreatedBy : entity.ModifiedBy;
                PropertyInfo IdProperty = entity.GetType().GetProperty(idName);

                if (IdProperty != null && idValue == "0")
                {
                    idValue = IdProperty.GetValue(entity).ToString();
                }
                else //if table+id is not the key column (template,submission etc only has Id as the key)
                {
                    IdProperty = entity.GetType().GetProperty("Id");
                    idValue = IdProperty.GetValue(entity).ToString();
                    idName = "Id";
                }

                if (!string.IsNullOrEmpty(idValue) && idValue != "0")
                {
                    var conn = gdct.Database.GetDbConnection();
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    DbCommand command = conn.CreateCommand();
                    command.CommandText = "dbo.sp_AddGDTCAuditLog";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("TableName", tableName));
                    command.Parameters.Add(new SqlParameter("PrimaryKeyName", idName));
                    command.Parameters.Add(new SqlParameter("PrimaryKeyValue", idValue));
                    command.Parameters.Add(new SqlParameter("LogStep", logStep));
                    command.Parameters.Add(new SqlParameter("Comments", ""));
                    command.Parameters.Add(new SqlParameter("UpdatedBy", updateBy));

                    //db.Database.OpenConnection();
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw new RepositoryException("An unexpected error occurred while getting all items for " + entity.GetType().Name, ex);
            }            
        }

        /***************************************************************
         * Function Name: GetPrimaryKey
         * Created By   : Jet Su  
         * Description  : Get Primary Key for insert entity
         * parameters   : None
         * -------------------------------------------------------------
         * History : 2025-02-28 Initial created - Jet 
         *         : 2025-03-14 Fix the Ambiguous match issue - Jet 
         *         : 2025-06-02 Add check null and try catch - Jet 
         ***************************************************************/
        private async Task<long> GetPrimayKeyValue(T entity, ShoppingCartContext gdct)
        {
            long result = 0;
            try
            {
                if (entity is not null)
                {
                    string? keyName = gdct.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(x => x.Name).FirstOrDefault();

                    if (!string.IsNullOrEmpty(keyName))
                    {
                        var value = entity.GetType().GetProperty(keyName)?.GetValue(entity, null);
                        if (value != null)
                        {
                            result = Convert.ToInt64(value);
                        }
                    }
                }
            }
            catch (Exception ex)
            { 
                _logger.LogError(ex.ToString());
                result = 0;
            }
            return result;
        }
    }
}
