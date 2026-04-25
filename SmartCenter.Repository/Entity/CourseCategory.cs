using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class CourseCategory: BaseEntity<Guid>, IAuditableEntity
{
    
    public Guid CourseId { get; set; }
    public Course Course { get; set; }
    
    public Guid CategoryId { get; set; }
    public Category Category { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

