using EcorpAPI.Models;

namespace EcorpAPI.Services.CartService
{
    public interface ICartService
    {
        Task<List<CartItemDetails>> GetCartItemsAsync(int? userId);
        Task<ResponseModel> AddToCartAsync(CartItemModel? cartItem);
        Task<ResponseModel> UpdateQuantityAsync(CartItemModel? cartItemMode);
        Task<bool> RemoveFromCartAsync(int? userId, int? cartItemId);
        Task<decimal> GetCartTotalAsync(int? userId);

    }
}
