using EcorpAPI.Models;
using EcorpAPI.Services;
using EcorpAPI.Services.ItemService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcorpAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemController : Controller
    {
        private readonly IItemService _itemService;
        private readonly ILogger<ItemController> _logger;
        public ItemController(IItemService itemService, ILogger<ItemController> logger) 
        {
            _itemService = itemService;
            _logger = logger;
        }

        [Route("GetItemsListForDashboard")]
        [HttpGet,Authorize]
        public async Task<IActionResult> GetItemsListForDashboard()
        {
            return Ok(await _itemService.GetItemList(false));
        }

        [Route("GetItemsListForShopPage")]
        [HttpGet]
        public async Task<IActionResult> GetItemsListForShopPage()
        {
            return Ok(await _itemService.GetItemList());
        }

        [Route("GetItemDetail/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetItemDetail(int? id)
        {
            return Ok(await _itemService.GetItemDetail(id));
        }

        [Route("AddItem")]
        [HttpPost,Authorize]
        public async Task<IActionResult> AddItemAsync(DetailedShoppingItem item)
        {
            return Ok(await _itemService.AddItemAsync(item));
        }

        [Route("EditItem")]
        [HttpPost,Authorize]
        public async Task<IActionResult> EditItemAsync(DetailedShoppingItem item)
        {
            return Ok(await _itemService.EditItemAsync(item));
        }

        [Route("DeleteItem/{item}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteItemAsync(int? item)
        {
            return Ok(await _itemService.DeleteItemAsync(item));
        }

       
    }
}
