using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class Combo: BaseEntity<Guid>, IAuditableEntity
{
    public string Name { get; set; }
    public int DiscountPercent { get; set; }
    public bool IsActive { get; set; }
    
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<ComboCourse> ComboCourses { get; set; } = new List<ComboCourse>();
    
    public ICollection<CartItem>? CartItems { get; set; } = new List<CartItem>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}