using EcorpAPI.Models;

namespace EcorpAPI.Services.ItemService
{
    public interface IItemService
    {
        Task<List<DetailedShoppingItem>> GetItemList(bool isShopPage = true);
        Task<DetailedShoppingItem> GetItemDetail(int? id);
        Task<ResponseModel> AddItemAsync(DetailedShoppingItem shoppingItem);
        Task<ResponseModel> EditItemAsync(DetailedShoppingItem shoppingItem);
        Task<ResponseModel> DeleteItemAsync(int? shoppingItemId);
        Task<List<DetailedShoppingItem>> GetItemImage(List<DetailedShoppingItem> shoppingItems);
    }
}
