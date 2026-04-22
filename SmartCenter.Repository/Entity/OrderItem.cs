using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class OrderItem: BaseEntity<Guid>, IAuditableEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
    
    public Guid ComboId { get; set; }
    public Combo Combo { get; set; }
    
    public Guid CourseId { get; set; }
    public Course Course { get; set; }
    
    public required string ItemName { get; set; }
    public required decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}