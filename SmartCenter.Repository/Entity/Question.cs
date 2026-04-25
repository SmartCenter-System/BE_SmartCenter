using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class Question: BaseEntity<Guid>, IAuditableEntity
{
    
    public required string Title { get; set; }
    public required string TypeOfQuestion { get; set; }
    public required int Point { get; set; }
    
    public ICollection<ExamPaperDetail> ExamPaperDetails { get; set; } = new List<ExamPaperDetail>();
    public ICollection<MultipleChoiceAnswer> MultipleChoiceAnswers { get; set; } = new List<MultipleChoiceAnswer>();
    public ICollection<EssayAnswer> EssayAnswers { get; set; } = new List<EssayAnswer>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}