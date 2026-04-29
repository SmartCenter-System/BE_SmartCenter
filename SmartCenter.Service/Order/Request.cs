namespace SmartCenter.Service.Order;

public class Request
{
    public class CreateOrderRequest
    {
        public Guid StudentId { get; set; }
        public Guid CartId { get; set; }
    }
}