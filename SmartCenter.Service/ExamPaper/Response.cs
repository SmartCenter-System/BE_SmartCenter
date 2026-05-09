using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.ExamPaper;

public class Response
{
    public class ExamResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = String.Empty;
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

    public class AddMultipleQuestionsResponse
    {
        public List<QuestionDetailResponse> AddedQuestions { get; set; } = new List<QuestionDetailResponse>();
        public decimal NewAveragePoint { get; set; } 
    }

    public class QuestionDetailResponse
    {
        public Guid QuestionId { get; set; }
        public string Title { get; set; } = null!;
        public QuestionType TypeOfQuestion { get; set; } 
        public List<AnswerOptionResponse>? MultipleChoiceAnswers { get; set; }
        public string? EssayContext { get; set; }
    }

    public class AnswerOptionResponse
    {
        public Guid AnswerId { get; set; }
        public string Content { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }
}