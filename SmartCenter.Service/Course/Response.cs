using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.Course;

public class Response
{
    public class CourseItemResponse
    {
        public Guid CateId { get; set; }
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public CourseType Mode  { get; set; }
        public decimal Price { get; set; }
        public int AvailableSlots { get; set; }
    }
    
    public class CourseDetailResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public CourseType Mode { get; set; }
        public int AvailableSlots { get; set; }

        public List<SectionResponse> Sections { get; set; } = new();
    }
    
    
    public class SectionResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public List<LessonResponse> Lessons { get; set; } = new();
    }
    
    public class LessonResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsPreview { get; set; }
    }
    
    public class DashboardResponse
    {
        public int NumberStundentsEnrolled { get; set; }
        public int Max_Enrolled { get; set; }
    }
    
    public class RecentReviewResponse
    {
        public Guid ReviewId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public string ImgUrl { get; set; }

        public Guid StudentId { get; set; }
        public string StudentName { get; set; } 
    }
}