using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class ExamManementDetail: BaseEntity<Guid>, IAuditableEntity
{
    
    public Guid ExamManementId { get; set; }
    public ExamManament ExamManement { get; set; }
    
    public Guid ExamPaperDetailId { get; set; }
    public ExamPaperDetail ExamPaperDetail { get; set; }
    
    public Guid? MultipleChoiceAnswerId { get; set; }
    public MultipleChoiceAnswer? MultipleChoiceAnswer { get; set; }
    
    public string? Answer { get; set; }
    public decimal? Point  { get; set; }
    public bool IsMultiChoice { get; set; }
    public string? Feedback { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}