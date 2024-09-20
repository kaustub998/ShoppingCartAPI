using Microsoft.EntityFrameworkCore;

namespace EcorpAPI.Models
{
    public class ConfirmedOrder
    {
        public int ConfirmedOrderId { get; set; }
        public int? BuyerId { get; set; }
        public int? ItemId { get; set; }
        [Precision(10,2)]
        public decimal? Rate { get; set; }
        public int? Quantity { get; set; }
        public DateTime? BoughtDate { get; set; }
        public string? TransactionId { get; set; }
        public virtual ShoppingItem? Item { get; set; }
        public virtual UserDetails? Buyer { get; set; }
    }
}
