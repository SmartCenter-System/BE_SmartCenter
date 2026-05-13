namespace SmartCenter.Service.Lesson;

public class Response
{
    public class LessonResponse
    {
        public Guid Id { get; set; }
        public Guid? ExamId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string VideoUrl { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsPreview { get; set; }
        public int Duration { get; set; }
    }
}