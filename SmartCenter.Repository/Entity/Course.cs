using SmartCenter.Repository.Abtraction;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Repository.Entity;

public class Course: BaseEntity<Guid>, IAuditableEntity
{
    
    public Guid LecId { get; set; }
    public Lecturer Lecturer { get; set; }
    
    
    public required string CourseName { get; set; }
    public required string Description { get; set; }
    public required decimal BasePrice { get; set; }
    public CourseType CourseType { get; set; } // onl, off
    public required string ImgUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public int MaxStudents { get; set; }
    public int AcademicYear { get; set; }
    
    
    public ICollection<Section> Sections { get; set; } = new List<Section>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<ComboCourse> ComboCourses { get; set; } = new List<ComboCourse>();
    public ICollection<ReviewCourse> Reviews { get; set; } =  new List<ReviewCourse>();
    
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public ICollection<CourseCategory> CourseCategories { get; set; } = new List<CourseCategory>();
    
    public ICollection<ConsultationRequest> ConsultationRequests { get; set; } = new List<ConsultationRequest>();
    
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}