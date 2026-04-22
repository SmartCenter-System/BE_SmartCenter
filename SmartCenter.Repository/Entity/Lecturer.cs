using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class Lecturer: BaseEntity<Guid>, IAuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    public required string Bio { get; set; }
    public required string Expertise { get; set; }
    
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}