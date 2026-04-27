using SmartCenter.Repository.Abtraction;

namespace SmartCenter.Repository.Entity;

public class Student: BaseEntity<Guid>, IAuditableEntity
{
    
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    public Guid CartId { get; set; }
    public Cart? Cart { get; set; }
    
    public string? Address { get; set; }
    public string? City { get; set; }
    public DateTimeOffset EnrollmentDate { get; set; }
    public string? ZaloLink { get; set; }
    
    public ICollection<Enrollment> Enrollments { get; set; } =  new List<Enrollment>();
    public ICollection<Order> Orders { get; set; } =  new List<Order>();
    
    public ICollection<ReviewCourse> Reviews { get; set; } =  new List<ReviewCourse>();
    public ICollection<LearningProcess> LearningProcesses { get; set; } =  new List<LearningProcess>();
    public ICollection<ExamManament> ExamManaments { get; set; } =  new List<ExamManament>();
    
    public ICollection<ReviewCourse>? ReviewCourses { get; set; } =  new List<ReviewCourse>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}