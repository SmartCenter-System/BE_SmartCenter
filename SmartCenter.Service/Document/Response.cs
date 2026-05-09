namespace SmartCenter.Service.Document;

public class Response
{
    public class DocumentResponse
    {
        public Guid DocumentId { get; set; }
        public Guid LessonId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}