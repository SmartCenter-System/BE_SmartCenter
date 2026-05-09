namespace SmartCenter.Service.Progress;

public class Request
{
    public class LessonProgressRequest
    {
        public required Guid LessonId { get; set; }
        public int WatchTime { get; set; } = 0; // tính bằng giây
    }
}