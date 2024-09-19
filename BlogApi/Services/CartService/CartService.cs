using EcorpAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcorpAPI.Services.CartService
{
    public class CartService : ICartService
    {
        private readonly ShoppingCartContext _shoppingCartContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        public CartService(ShoppingCartContext shoppingCartContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _shoppingCartContext = shoppingCartContext;
        }


        public async Task<List<CartItemModel>> GetCartItemsAsync(int? userId)
        {
            return await _shoppingCartContext.CartItems
                .Where(ci => ci.UserId == userId)            
                .Include(ci => ci.Item)                    
                .ToListAsync();                       
        }
        public async Task<ResponseModel> AddToCartAsync(CartItemModel? cartItemModel)
        {
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

            // Reduce the stock quantity of the item
            item.ItemQuantity -= cartItemModel.Quantity;

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


        public async Task<bool> RemoveFromCartAsync(int? userId, int? cartItemId)
        {
            var cartItem = await _shoppingCartContext.CartItems
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.UserId == userId); 

            if (cartItem == null)  
                return false;

            var item = await _shoppingCartContext.ShoppingItems.FindAsync(cartItem.ItemId);  
            if (item != null)
            {
                item.ItemQuantity += cartItem.Quantity;  // Restore the stock quantity of the item
            }

            _shoppingCartContext.CartItems.Remove(cartItem); 
            return await _shoppingCartContext.SaveChangesAsync() > 0;  
        }

        public async Task<bool> UpdateQuantityAsync(CartItemModel? cartItemModel)
        {
            var cartItem = await _shoppingCartContext.CartItems
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemModel.CartItemId && ci.UserId == cartItemModel.UserId);

            if (cartItem == null)
                return false;

            // Find the item in the inventory
            var item = await _shoppingCartContext.ShoppingItems.FindAsync(cartItem.ItemId);
            if (item == null)
                return false;

            var quantityDifference = cartItemModel?.Quantity - cartItem.Quantity;  // Calculate the difference in quantity
            if (item.ItemQuantity < quantityDifference)  // If the inventory doesn't have enough stock, return false
                return false;

            // Adjust the stock quantity
            item.ItemQuantity -= quantityDifference;

            // Update the cart item quantity
            cartItem.Quantity = cartItemModel.Quantity;

            return await _shoppingCartContext.SaveChangesAsync() > 0;
        }

        public async Task<decimal> GetCartTotalAsync(int? userId)
        {
            var total = await _shoppingCartContext.CartItems
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.Item)
                .SumAsync(ci => (decimal?)ci.Item.ItemRate * ci.Quantity);

            return total ?? 0; 
        }



    }
}
