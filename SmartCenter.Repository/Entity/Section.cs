using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class Section: BaseEntity<Guid>, IAuditableEntity
{
    public Guid CourseId { get; set; }
    public Course Course { get; set; }
    
    public required string Title { get; set; }
    public required int Position { get; set; }
    public bool IsActive { get; set; }
    
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}