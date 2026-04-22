using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class Lesson: BaseEntity<Guid>, IAuditableEntity
{
    
    public Guid SectionId { get; set; }
    public Section Section { get; set; }
    
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string VideoUrl { get; set; }
    public required int Duration { get; set; }
    public bool IsPreview { get; set; }
    public int Position { get; set; }
    
    public ICollection<LearningProcess> LearningProcesses { get; set; } =  new List<LearningProcess>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}