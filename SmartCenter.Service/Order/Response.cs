namespace SmartCenter.Service.Order;

public class Response
{
    public class OrderResponse
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;

        public decimal SubtotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;

        public DateTimeOffset ExpireAt { get; set; }
        public DateTimeOffset? PaidAt { get; set; }

        public List<OrderItemResponse> Items { get; set; } = new();
    }
    
    public class OrderItemResponse
    {
        public Guid? CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}