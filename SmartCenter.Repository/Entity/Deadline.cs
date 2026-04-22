using SmartCenter.Repository.Abtraction;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Repository.Entity;

public class Deadline: BaseEntity<Guid>, IAuditableEntity
{
    public Guid ExamPaperId { get; set; }
    public ExamPaper ExamPaper { get; set; }
    
   public required string Title { get; set; }
   public DeadlineStatus Status { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset EndedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}