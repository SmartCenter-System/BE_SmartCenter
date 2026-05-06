using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.ExamPaper;

public class Response
{
    public class ExamResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } =  String.Empty;
        public int CountDown { get; set; } 
        public decimal TotalPoints { get; set; }
        public ExamPaperStatus Status { get; set; }
        public Guid LessonId { get; set; }
        public DeadlineResponse? Deadline { get; set; }
        public DateTimeOffset? CreateAt { get; set; }
    }
    public class DeadlineResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTimeOffset EndedAt { get; set; }
        public DeadlineStatus Status { get; set; } = DeadlineStatus.Processing;
    }
}