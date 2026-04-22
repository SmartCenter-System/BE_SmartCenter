using SmartCenter.Repository.Abtraction;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Repository.Entity;

public class ExamPaper: BaseEntity<Guid>, IAuditableEntity
{
    
    public Guid DeadlineId { get; set; }
    public Deadline Deadline { get; set; }
    
    public Guid LecturerId { get; set; }
    public Lecturer Lecturer { get; set; }
    
    public required string Title { get; set; }
    public int CountDown { get; set; }
    public int TotalPoints { get; set; }
    public ExamPaperStatus Status { get; set; }
    
    public ICollection<ExamManament> ExamManaments { get; set; } =  new List<ExamManament>();
    public ICollection<ExamComment> ExamComments { get; set; } =  new List<ExamComment>();
    public ICollection<ExamPaperDetail> ExamPaperDetails { get; set; } =  new List<ExamPaperDetail>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}