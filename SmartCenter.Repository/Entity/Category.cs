using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class Category: BaseEntity<Guid>, IAuditableEntity
{
    
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string IconUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CourseCategory> CourseCategories { get; set; } = new List<CourseCategory>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}