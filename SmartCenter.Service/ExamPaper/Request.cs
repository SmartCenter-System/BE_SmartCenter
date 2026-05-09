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

    public class AddMultipleQuestionsRequest
    {
        public List<QuestionItemRequest> Questions { get; set; } = new List<QuestionItemRequest>();
    }

    public class QuestionItemRequest
    {
        public string Title { get; set; } = null!;
        public QuestionType TypeOfQuestion { get; set; }

        public List<AnswerOptionRequest>? MultipleChoiceAnswers { get; set; }

        public string? EssayContext { get; set; }
    }

    public class AnswerOptionRequest
    {
        public string Content { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }
}