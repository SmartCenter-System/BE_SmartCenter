using SmartCenter.Repository.Abtraction;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Repository.Entity;

public class Enrollment: BaseEntity<Guid>, IAuditableEntity
{
    
    public Guid StuId { get; set; }
    public Student Student { get; set; }
    
    public Guid CourseId { get; set; }
    public Course Course { get; set; }
    
    public Guid TransactionId { get; set; }
    public Transaction Transaction { get; set; }
    
    public DateTimeOffset EnrollmentDate { get; set; }
    public EnrollmentStatus Status { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}