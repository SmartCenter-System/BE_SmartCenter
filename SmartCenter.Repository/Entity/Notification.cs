using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class Notification: BaseEntity<Guid>, IAuditableEntity
{
    
    public User? User { get; set; }
    public Guid UserId { get; set; }
    
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Type { get; set; }
    public required Guid RefId { get; set; }
    public required string RefType { get; set; }
    public bool IsRead { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

