using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class ExamPaperDetail: BaseEntity<Guid>, IAuditableEntity
{
    public Guid ExamPaperId { get; set; }
    public ExamPaper ExamPaper { get; set; }
    
    public Guid QuestionId { get; set; }
    public Question Question { get; set; }
    
    public ICollection<ExamManementDetail> ExamManementDetails { get; set; } = new List<ExamManementDetail>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}