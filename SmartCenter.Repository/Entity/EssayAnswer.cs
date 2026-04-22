using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class EssayAnswer: BaseEntity<Guid>, IAuditableEntity
{
    public Guid QuestionId { get; set; }
    public Question Question { get; set; }
    
    public required string Content { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}