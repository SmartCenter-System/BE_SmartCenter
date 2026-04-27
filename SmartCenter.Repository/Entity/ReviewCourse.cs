using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class ReviewCourse: BaseEntity<Guid>, IAuditableEntity
{
    
    public Guid CourseId { get; set; }
    public Course Course { get; set; }
    
    public Guid StuId { get; set; }
    public Student Student { get; set; }
    
    public required int Rating { get; set; }
    public required string? Comment { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}