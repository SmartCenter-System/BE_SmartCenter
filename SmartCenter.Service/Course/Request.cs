using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.Course;

public class Request
{
    public class CourseFilterRequest
    {
        public Guid? CategoryId { get; set; }
        public Guid? CourseId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public CourseType? Mode { get; set; } //online or offline
        public string? Keyword { get; set; }
        
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
    
    public class CreateCourseRequest
    {
        public required string CourseName { get; set; }
        public required string Description { get; set; }
        public required decimal BasePrice { get; set; }
        public required string ImgUrl { get; set; }
        public CourseType CourseType { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public int MaxStudents { get; set; }
        public int AcademicYear { get; set; }
    }

    public class UpdateCourseRequest
    {
        public string? CourseName { get; set; }
        public string? Description { get; set; }
        public decimal? BasePrice { get; set; }
        public string? ImgUrl { get; set; }
        public DateTimeOffset? StartAt { get; set; }
        public DateTimeOffset? EndAt { get; set; }
        public int? MaxStudents { get; set; }
        public bool? IsActive { get; set; }
    }
}