namespace SmartCenter.Service.ReviewCourseService;

public class Response
{
    public class ReviewResponse
    {
        public Guid ReviewId { get; set; }
        public Guid CourseId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
    
    public class ReviewDetailResponse
    {
        public Guid ReviewId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
 
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
    }
}