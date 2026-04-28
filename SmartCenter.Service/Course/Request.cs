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
}