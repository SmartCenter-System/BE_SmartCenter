using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class Transaction: BaseEntity<Guid>, IAuditableEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
    
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    
    public decimal Amount { get; set; }
    
    public required string ProviderTransactionCode { get; set; }
    public required Guid ConfirmedByStaffId { get; set; }
    public DateTimeOffset ConfirmedAt { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}