using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class Cart: BaseEntity<Guid>
{
    public Guid StuId { get; set; }
    public Student Student { get; set; }
    
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    
}