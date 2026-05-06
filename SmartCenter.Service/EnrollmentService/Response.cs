using SmartCenter.Repository.Entity;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.EnrollmentService;

public class Response
{
    public class MyEnrollmentResponse
    {
        public string CourseName { get; set; }
        
        public decimal BasePrice { get; set; }
        
        public CourseType CourseType { get; set; }
        
        public string ImgUrl { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTimeOffset StartAt { get; set; }
        
        public DateTimeOffset EndAt { get; set; }
        
        public int AcademicYear { get; set; }
        public DateTimeOffset EnrollmentDate { get; set; }
        public EnrollmentStatus status { get; set; }
    }
}