using Microsoft.EntityFrameworkCore;

namespace EcorpAPI.Models
{
    public class CartItemModel
    {
        public int CartItemId { get; set; }  
        public int? ItemId { get; set; }      
        public int? UserId { get; set; }       
        public int? Quantity { get; set; }   
        public virtual ShoppingItem? Item { get; set; }

    }
    public class CartItemDetails: CartItemModel
    {
        public string? ItemName { get; set; }
        public string? ItemDescription { get; set; }
        public decimal ItemRate { get; set; }
        public List<ItemImage>? ItemImageList { get; set; }

    }


}
