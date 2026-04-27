using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class Document:BaseEntity<Guid>,IAuditableEntity
{
    public string FileName { get; set; }
    
    public string FileUrl { get; set; }
    
    public string FileType { get; set; }
    
    public Lesson Lesson { get; set; }
    
    public Guid LessonId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}