namespace SmartCenter.Service.Progress;

public class Response
{
    public class CourseProgressResponse
    {
        public double Percent { get; set; }
        public int CompletedCount { get; set; }
        public int TotalCount { get; set; }
    }
}