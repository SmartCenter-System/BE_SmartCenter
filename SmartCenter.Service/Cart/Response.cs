using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.Cart;

public class Response
{
    public class CartItemResponse
    {
        public required string ItemName { get; set; }
        public string? ItemDescription { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal ItemAmount { get; set; }
        public CartItemType ItemType { get; set; }
        public int Quantity { get; set; }
        
    }
}