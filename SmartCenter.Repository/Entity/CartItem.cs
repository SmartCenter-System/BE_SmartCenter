using SmartCenter.Repository.Abtraction;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Repository.Entity;

public class CartItem: BaseEntity<Guid>, IAuditableEntity
{
    
    public Guid CartId { get; set; }
    public Cart Cart { get; set; }
    
    public Course Course { get; set; }
    public Guid CourseId { get; set; }
    
    public Combo Combo { get; set; }
    
    public Guid ComboId { get; set; }
    
    public CartItemType ItemType { get; set; }
    

    public int Quantity { get; set; } = 1;
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}