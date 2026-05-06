using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.Cart;

public class Request
{
    public class AddItemToCartRequest
    {
        public Guid ItemId { get; set; }
        public int Quantity { get; set; }
        public CartItemType ItemType { get; set; }
    }

    public class RemoveItemFromCartRequest
    {
        public Guid ItemId { get; set; }
    }
}