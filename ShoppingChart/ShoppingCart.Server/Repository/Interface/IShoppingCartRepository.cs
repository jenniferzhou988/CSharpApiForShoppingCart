using ShoppingCartAPI.Models;

namespace ShoppingCartAPI.Repository.Interface
{
    public interface IShoppingCartRepository : IGDCTRepository<Models.ShoppingCart>
    {
        Task<Models.ShoppingCart> CreateAsync(int customerId);
        Task<Models.ShoppingCart?> GetByIdAsync(int id);
        Task<IEnumerable<Models.ShoppingCart>> GetAllAsync();
        Task<bool> DeleteAsync(int id);
        Task<ShoppingCartDetail?> AddItemAsync(int shoppingCartId, int productId, int quantity);
        Task<ShoppingCartDetail?> UpdateItemAsync(int shoppingCartId, int detailId, int quantity);
        Task<bool> RemoveItemAsync(int shoppingCartId, int detailId);
    }
}