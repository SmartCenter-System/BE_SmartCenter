namespace SmartCenter.Service.Lesson;

public class Request
{
    public class CreateLessonRequest
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required string VideoUrl { get; set; }
        public int Order { get; set; }
        public bool IsPreview { get; set; } = false;
        public int Duration { get; set; }
    }

    public class UpdateLessonRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? VideoUrl { get; set; }
        public int? Order { get; set; }
        public bool? IsPreview { get; set; }
        
        public int? Duration { get; set; }
    }
}