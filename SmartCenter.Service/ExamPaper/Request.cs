using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.ExamPaper;

public class Request
{
    public class CreateExamPaperRequest
    {
        public required string Title { get; set; }
        public int CountDown { get; set; }
        public decimal TotalPoints { get; set; }
        public Guid LessonId { get; set; }
    }
    
    public class UpdateExamPaperRequest
    {
        public required string? Title { get; set; }
        public int? CountDown { get; set; }
        public decimal? TotalPoints { get; set; }
        public ExamPaperStatus? Status { get; set; }
    }
    public class SetDeadlineRequest
    {
        public required string Title { get; set; }
        public DateTimeOffset EndedAt { get; set; }
    }
}