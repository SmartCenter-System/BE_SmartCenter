namespace SmartCenter.Service.Financial;

public class Response
{
    public class TransactionResponse
    {
        public Guid TransactionId { get; set; }
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ProviderTransactionCode { get; set; } = string.Empty;
        public DateTimeOffset? ConfirmedAt  { get; set; }
        public DateTimeOffset? CreatedAt  { get; set; }
    }
    
    public class OrderDetailResponse
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public decimal SubtotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset? PaidAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<OrderItemDetail> Items { get; set; } = new();
        public TransactionDetail? Transaction { get; set; }
    }
    
    public class OrderItemDetail
    {
        public Guid OrderItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total => UnitPrice * Quantity;
        public Guid? CourseId { get; set; }
    }
    
    public class TransactionDetail
    {
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ProviderTransactionCode { get; set; } = string.Empty;
        public DateTimeOffset ConfirmedAt { get; set; }
    }
    
    public class RevenueByPeriodResponse
    {
        public string PeriodLabel { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TransactionCount { get; set; }
    }
 
    public class RevenuePerCourseResponse
    {
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
    }
}