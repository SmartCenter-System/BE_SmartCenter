using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class LearningProcess: BaseEntity<Guid>, IAuditableEntity
{
    
    public Guid StuId { get; set; }
    public Student Student { get; set; }
    
    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; }
    
    public required int WatchTime {get; set;}
    public bool IsCompleted { get; set; }
    public DateTimeOffset LastWatchedAt { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

