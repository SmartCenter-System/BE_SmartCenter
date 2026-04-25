using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class MultipleChoiceAnswer: BaseEntity<Guid>, IAuditableEntity
{
    
    public Guid QuestionId { get; set; }
    public Question Question { get; set; }
    
    public required string Content { get; set; }
    public bool IsCorrect { get; set; }
    
    public ICollection<ExamManementDetail> ExamManementDetails { get; set; } = new List<ExamManementDetail>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}