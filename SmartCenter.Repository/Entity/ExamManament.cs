using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class ExamManament: BaseEntity<Guid>, IAuditableEntity
{
    public Guid ExamPaperId { get; set; }
    public ExamPaper ExamPaper { get; set; }
    
    public Guid StudentId { get; set; }
    public Student Student { get; set; }
    
    public required int PointsOfStudent { get; set; }
    
    public ICollection<ExamManementDetail> ExamManementDetails { get; set; } = new List<ExamManementDetail>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}