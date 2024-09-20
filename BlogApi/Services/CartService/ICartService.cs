using EcorpAPI.Models;

namespace EcorpAPI.Services.CartService
{
    public interface ICartService
    {
        Task<List<CartItemModel>> GetCartItemsAsync(int? userId);
        Task<ResponseModel> AddToCartAsync(CartItemModel? cartItem);
        Task<bool> UpdateQuantityAsync(CartItemModel? cartItemMode);
        Task<bool> RemoveFromCartAsync(int? userId, int? cartItemId);
        Task<decimal> GetCartTotalAsync(int? userId);
        Task<ResponseModel> CheckOutCart();
        Task<List<ConfirmedOrder>> GetSoldItemsDetail();
    }
}
