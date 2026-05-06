namespace SmartCenter.Service.ExamManagementService;

public class Request
{
    public class SubmitExamRequest
    {
        public Guid ExamId { get; set; }
        public List<AnswerItems> ListAnswers { get; set; } = new List<AnswerItems>();
    }

    public class AnswerItems
    {
        public required Guid QuestionId { get; set; }
        public List<Guid>? MultipleChoiceAnswerIds { get; set; } = new List<Guid>();
        public string? AnswerText { get; set; }
        public bool is_MultipleChoice { get; set; } = true;
    }
}