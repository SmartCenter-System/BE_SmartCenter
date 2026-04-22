using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class ComboCourse: BaseEntity<Guid>, IAuditableEntity
{
    public Guid CourseId { get; set; }
    public Course Course { get; set; }
    
    public Guid ComboId { get; set; }
    public Combo Combo { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}