namespace SmartCenter.Service.Order;

public interface IService
{
    Task<Response.OrderResponse> CreateOrderAsync(Guid studentId, Guid cartId);

    Task<Response.OrderResponse?> GetOrderByIdAsync(Guid orderId);

    Task<List<Response.OrderResponse>> GetOrdersByUserAsync(Guid studentId);

    Task CancelOrderAsync(Guid orderId, Guid studentId);
}