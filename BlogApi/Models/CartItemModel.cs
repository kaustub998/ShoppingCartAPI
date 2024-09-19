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
    

}
