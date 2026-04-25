using SmartCenter.Repository.Abtraction;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Repository.Entity;

public class Order: BaseEntity<Guid>, IAuditableEntity
{
    public Guid StuId { get; set; }
    public Student Student { get; set; }
    
    public Guid TransactionId { get; set; }
    public Transaction Transaction { get; set; }
    
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    
    public required string OrderCode { get; set; }
    public required decimal SubtotalAmount { get; set; }
    public required decimal DiscountAmount { get; set; }
    public required decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset ExpireAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}