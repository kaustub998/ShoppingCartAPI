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
        public virtual UserDetails? UserDetails { get; set; }
        public virtual ItemImage? ItemImages { get; set; }
    }

    public class DetailedShoppingItem : ShoppingItem
    {
        public string? User_FullName { get; set; }
        public List<ItemImageDetailed>? ItemImageList { get; set; }
    }

    public class ItemImage : BaseModel
    {
        public int ImageId { get; set; }
        public int? ItemId { get; set; }
        public string? ImagePath { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class ItemImageDetailed : ItemImage
    {
        public byte[]? ImageBytes { get; set; }
    }
}
