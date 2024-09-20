using EcorpAPI.Models;
using EcorpAPI.Services;
using EcorpAPI.Services.CartService;
using EcorpAPI.Services.ItemService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcorpAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;
        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        [Route("GetCartItems")]
        [HttpGet]
        public async Task<IActionResult> GetCartItems([FromQuery] int? userId)
        {
            return Ok(await _cartService.GetCartItemsAsync(userId));
        }

        [Route("AddToCart")]
        [HttpPost, Authorize]
        public async Task<IActionResult> AddToCart([FromBody] CartItemModel? cartItem)
        {
            return Ok(await _cartService.AddToCartAsync(cartItem));
        }

        [Route("UpdateQuantity")]
        [HttpPost, Authorize]
        public async Task<IActionResult> UpdateQuantity([FromBody] CartItemModel? cartItemMode)
        {
            return Ok(await _cartService.UpdateQuantityAsync(cartItemMode));
        }

        [Route("RemoveFromCart")]
        [HttpDelete]
        public async Task<IActionResult> RemoveFromCart([FromQuery]int? userId, [FromQuery] int? CartItemId)
        {
            return Ok(await _cartService.RemoveFromCartAsync(userId, CartItemId));
        }

        [Route("GetCartTotal")]
        [HttpGet]
        public async Task<IActionResult> GetCartTotal(int? userId)
        {
            return Ok(await _cartService.GetCartTotalAsync(userId));
        }

        [Route("CheckoutCart")]
        [HttpGet]
        public async Task<IActionResult> CheckoutCart()
        {
            return Ok(await _cartService.CheckOutCart());
        }

    }
}

