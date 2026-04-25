using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class ExamComment: BaseEntity<Guid>,IAuditableEntity
{
    public Guid ParentExamCommentId { get; set; }
    
    public Guid ExamPaperId { get; set; }
    public ICollection<ExamComment> Comments { get; set; } =  new List<ExamComment>();
    
    public ExamPaper ExamPaper { get; set; }
    
    public string? Content { get; set; }
    public int NumberOfLikes { get; set; }
    public int NumberOfDislikes { get; set; }
    
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}