namespace SmartCenter.Service.Order;

public interface IService
{
    Task<Response.OrderResponse> CreateOrderAsync();

    Task<Response.OrderResponse?> GetOrderByIdAsync(Guid orderId);

    Task<List<Response.OrderResponse>> GetOrdersByUserAsync();

    Task CancelOrderAsync(Guid orderId);
}