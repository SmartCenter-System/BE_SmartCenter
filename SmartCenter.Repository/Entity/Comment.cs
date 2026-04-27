using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class Comment: BaseEntity<Guid>, IAuditableEntity
{
    public User User { get; set; }
    public Guid UserId { get; set; }
    
    public Guid? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment>? Replies { get; set; } = new List<Comment>();
    
    
    public Lesson Lesson { get; set; }
    public Guid LessonId { get; set; }
    
    public required string Content { get; set; }
    public int DepthLevel { get; set; }
    public bool IsLocked { get; set; }
    
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}