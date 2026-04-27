using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class AuditLog: BaseEntity<Guid>{
    
    public required string Action  { get; set; }
    public required string Entity { get; set; }
    public Guid EntityId { get; set; }
    public required string Metadata { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public User User { get; set; }
    public Guid UserId { get; set; }
}