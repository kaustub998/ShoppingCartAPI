using EcorpAPI.Models;

namespace EcorpAPI.Services.ItemService
{
    public interface IItemService
    {
        Task<List<DetailedShoppingItem>> GetItemList(bool isShopPage = true);
        Task<DetailedShoppingItem> GetItemDetail(int? id);
        Task<ResponseModel> AddItemAsync(AddEditShoppingItem shoppingItem);
        Task<ResponseModel> EditItemAsync(AddEditShoppingItem shoppingItem);
        Task<ResponseModel> DeleteItemAsync(int? shoppingItemId);
    }
}
