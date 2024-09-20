using Azure.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcorpAPI.Models
{
    public class ShoppingItem : BaseModel
    {
        public int ItemId { get; set; }
        public int? UserId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemDescription { get; set; }
        public int? ItemQuantity { get; set; }
        [Precision(10, 2)]
        public decimal ItemRate { get; set; }
        public bool IsDeleted { get; set; }
        public virtual ICollection<CartItemModel>? CartItems { get; set; }
        public virtual ICollection<ConfirmedOrder>? ConfirmedOrders { get; set; }
    }

    public class DetailedShoppingItem : ShoppingItem
    {
        public string? User_FullName { get; set; }
        public List<ItemImage>? ItemImageList { get; set; }
    }

    public class ItemImage : BaseModel
    {
        public int ImageId { get; set; }
        public int? ItemId { get; set; }
        public byte[]? Image { get; set; }
        public bool IsDeleted { get; set; }
    }
}
