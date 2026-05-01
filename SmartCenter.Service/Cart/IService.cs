namespace SmartCenter.Service.Cart;

public interface IService
{
    public Task CreateCart(Guid studentId);
    public Task<List<Response.CartItemResponse?>> GetCartItem(Guid studentId);
    public Task AddItemToCart(Request.AddItemToCartRequest request);
    public Task RemoveItemFromCart(Request.RemoveItemFromCartRequest request);
}