using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.Course;

public class Response
{
    public class CourseResponse()
    {
        public Guid LecturerId { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public CourseType CourseType { get; set; }
        public string ImgUrl { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAT { get; set; }
        public int Max_Students { get; set; }
        public int AcademicYear { get; set; }
    }
}