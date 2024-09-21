using Azure;
using EcorpAPI.Models;
using EcorpAPI.Services.ItemService;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EcorpAPI.Services.CartService
{
    public class CartService : ICartService
    {
        private readonly ShoppingCartContext _shoppingCartContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IItemService _itemService;
        public CartService(ShoppingCartContext shoppingCartContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IItemService itemService)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _shoppingCartContext = shoppingCartContext;
            _itemService = itemService;
        }


        public async Task<List<CartItemDetails>> GetCartItemsAsync(int? userId)
        {
            userId = CommonService.GetUserId(_httpContextAccessor.HttpContext);
            try
            {
                var data = await _shoppingCartContext.CartItems
                    .Where(ci => ci.UserId == userId)
                    .Include(ci => ci.Item).Select(x => new CartItemDetails
                    {
                        CartItemId = x.CartItemId,
                        ItemId = x.ItemId,
                        ItemName = x.Item.ItemName,
                        ItemDescription = x.Item.ItemDescription,
                        ItemRate = x.Item.ItemRate,
                        Quantity = x.Quantity,
                        UserId = x.UserId,
                    }).ToListAsync();
                var newData = data.Select(x => new DetailedShoppingItem
                {
                    ItemId = x.ItemId ?? 0,
                }).ToList();
                var ImageData = await _itemService.GetItemImage(newData);

                foreach (var cart in data)
                {
                    cart.ItemImageList = ImageData?.Where(x => x.ItemId == cart.ItemId).FirstOrDefault()?.ItemImageList;
                }
                return data;
            }
            catch (Exception ex)
            {
                return new List<CartItemDetails>();
            }
        }
        public async Task<ResponseModel> AddToCartAsync(CartItemModel? cartItemModel)
        {
            cartItemModel.UserId = CommonService.GetUserId(_httpContextAccessor.HttpContext);
            var response = new ResponseModel();

            // Find the item by ID
            var item = await _shoppingCartContext.ShoppingItems.FindAsync(cartItemModel.ItemId);
            if (item == null)
            {
                response.isSuccess = false;
                response.message = "Item not found.";
                return response;
            }

            // Check if the item has enough stock
            if (item.ItemQuantity < cartItemModel.Quantity)
            {
                response.isSuccess = false;
                response.message = "Not enough stock available.";
                return response;
            }

            // Find if the item is already in the cart
            var existingCartItem = await _shoppingCartContext.CartItems
                .FirstOrDefaultAsync(ci => ci.UserId == cartItemModel.UserId && ci.ItemId == cartItemModel.ItemId);

            if (existingCartItem == null)
            {
                // If the item isn't in the cart, create a new cart item
                var newCartItem = new CartItemModel
                {
                    UserId = cartItemModel.UserId,
                    ItemId = cartItemModel.ItemId,
                    Quantity = cartItemModel.Quantity
                };
                _shoppingCartContext.CartItems.Add(newCartItem);
            }
            else
            {
                // If the item is already in the cart, update the quantity
                existingCartItem.Quantity += cartItemModel.Quantity;
            }

            // Save changes and set response based on success
            var changes = await _shoppingCartContext.SaveChangesAsync();
            if (changes > 0)
            {
                response.isSuccess = true;
                response.message = "Item added to cart successfully.";
            }
            else
            {
                response.isSuccess = false;
                response.message = "Failed to add item to cart.";
            }

            return response;
        }


        public async Task<ResponseModel> RemoveFromCartAsync(int? userId, int? cartItemId)
        {
            var responseModel = new ResponseModel();
            userId = CommonService.GetUserId(_httpContextAccessor.HttpContext);
            try
            {
                var cartItem = await _shoppingCartContext.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.UserId == userId);

                if (cartItem == null)
                {
                    responseModel.isSuccess = false;
                    return responseModel;
                }
                    

                var item = await _shoppingCartContext.ShoppingItems.FindAsync(cartItem.ItemId);
                if (item != null)
                {
                    item.ItemQuantity += cartItem.Quantity;  // Restore the stock quantity of the item
                }

                _shoppingCartContext.CartItems.Remove(cartItem);
                await _shoppingCartContext.SaveChangesAsync();
                responseModel.isSuccess = true;
                return responseModel;
            }
            catch (Exception ex)
            {
                responseModel.isSuccess = false;
                return responseModel;
            }
        }

        public async Task<ResponseModel> UpdateQuantityAsync(CartItemModel? cartItemModel)
        {
            cartItemModel.UserId = CommonService.GetUserId(_httpContextAccessor.HttpContext);
            var response = new ResponseModel();
            try
            {

                var cartItem = await _shoppingCartContext.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemModel.CartItemId && ci.UserId == cartItemModel.UserId);

                if (cartItem == null)
                {
                    response.isError = true;
                    response.message = "Something Went Wrong!!!.";
                    return response;
                }

                // Find the item in the inventory
                var item = await _shoppingCartContext.ShoppingItems.FindAsync(cartItem.ItemId);
                if (item == null)
                {
                    response.isError = true;
                    response.message = "Something Went Wrong!!!.";
                    return response;
                }

                var quantityDifference = cartItemModel?.Quantity - cartItem.Quantity;  // Calculate the difference in quantity
                if (item.ItemQuantity < quantityDifference)  // If the inventory doesn't have enough stock, return false
                {
                    response.isError = true;
                    response.message = "Something Went Wrong!!!.";
                    return response;
                }

                // Adjust the stock quantity
                item.ItemQuantity -= quantityDifference;

                // Update the cart item quantity
                cartItem.Quantity = cartItemModel.Quantity;

                _shoppingCartContext.CartItems.Update(cartItem);
                await _shoppingCartContext.SaveChangesAsync();

                response.isSuccess = true;
                return response;
            }
            catch
            {

            }
            response.isError = true;
            response.message = "Something Went Wrong!!!.";
            return response;
        }

        public async Task<decimal> GetCartTotalAsync(int? userId)
        {
            userId = CommonService.GetUserId(_httpContextAccessor.HttpContext);
            var total = await _shoppingCartContext.CartItems
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.Item)
                .SumAsync(ci => (decimal?)ci.Item.ItemRate * ci.Quantity);

            return total ?? 0;
        }

        public async Task<ResponseModel> CheckOutCart()
        {
            var response = new ResponseModel();

            var userId = CommonService.GetUserId(_httpContextAccessor.HttpContext);
            var userCartItems = await _shoppingCartContext.CartItems.Where(item => item.UserId == userId).ToListAsync();

            var items = await _shoppingCartContext.ShoppingItems.Where(item => userCartItems.Select(x => x.ItemId).ToList().Contains(item.ItemId)).ToListAsync();

            var itemsExceedingStock = items.Where(item => userCartItems.Any(cartItem => cartItem.ItemId == item.ItemId && cartItem.Quantity > item.ItemQuantity)).ToList();

            if (itemsExceedingStock.Count() > 0)
            {
                response.isSuccess = false;
                response.isError = true;
                response.message = "Items in the cart exceed the total quantity.";
                return response;
            }
            else
            {
                var transactionId = (new Guid()).ToString();

                foreach (var item in items)
                {
                    if (item.ItemQuantity >= userCartItems.Where(x => x.ItemId == item.ItemId).FirstOrDefault()?.Quantity)
                    {
                        item.ItemQuantity -= userCartItems.Where(x => x.ItemId == item.ItemId).FirstOrDefault()?.Quantity;
                        _shoppingCartContext.ShoppingItems.Update(item);
                    }
                    else
                    {
                        response.isSuccess = false;
                        response.isError = true;
                        response.message = "Items in the cart exceed the total quantity.";
                        return response;
                    }
                }

                foreach (var cartItem in userCartItems)
                {
                    _shoppingCartContext.ConfirmedOrders.Add(new ConfirmedOrder
                    {
                        BuyerId = userId,
                        ItemId = cartItem.ItemId,
                        Quantity = cartItem.Quantity,
                        Rate = items.Where(x => x.ItemId == cartItem.ItemId).Select(x => x.ItemRate).FirstOrDefault(),
                        BoughtDate = DateTime.UtcNow,
                        TransactionId = transactionId,
                    });
                }

                _shoppingCartContext.CartItems.RemoveRange(userCartItems);
                await _shoppingCartContext.SaveChangesAsync();

                response.isSuccess = true;
                response.isError = false;
                response.message = "Items Bought.";
                return response;
            }

        }

        public async Task<List<DetailedConfirmedOrder>> GetSoldItemsDetail()
        {
            var userId = CommonService.GetUserId(_httpContextAccessor.HttpContext);

            var soldOrders = await _shoppingCartContext.ConfirmedOrders.Where(item => item.Item.UserId == userId)
                .Select(item => new DetailedConfirmedOrder
                {
                    BuyerId = item.BuyerId,
                    BuyerName = item.Buyer.FirstName + " " + item.Buyer.LastName,
                    ItemName = item.Item.ItemName,
                    ItemId = item.ItemId,
                    Rate = item.Rate,
                    Quantity = item.Quantity,
                }).ToListAsync();

            soldOrders = soldOrders.GroupBy(item => new { item.ItemId, item.BuyerId, item.Rate }).Select(item => new DetailedConfirmedOrder
            {
                BuyerId = item.Key.BuyerId,
                BuyerName = item.FirstOrDefault()?.BuyerName,
                ItemName = item.FirstOrDefault()?.ItemName,
                ItemId = item.Key.ItemId,
                Rate = item.Key.Rate,
                Quantity = item.Sum(x => x.Quantity),
                Total = item.Key.Rate * item.Sum(x => x.Quantity)
            }).ToList();

            var newData = soldOrders.Select(x => new DetailedShoppingItem
            {
                ItemId = x.ItemId ?? 0,
            }).ToList();
            var ImageData = await _itemService.GetItemImage(newData);

            foreach (var cart in soldOrders)
            {
                cart.ItemImageList = ImageData?.Where(x => x.ItemId == cart.ItemId).FirstOrDefault()?.ItemImageList;
            }

            return soldOrders;
        }

    }
}
