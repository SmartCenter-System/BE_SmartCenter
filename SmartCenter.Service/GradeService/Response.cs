namespace SmartCenter.Service.GradeService;

public class Response
{
    public class MyExamDetailsResponse
    {
        public Guid ExamId { get; set; }
        public string ExamTitle { get; set; }
        public double TotalScore { get; set; } 
        public List<ExamQuestionDetail> QuestionDetails { get; set; } = new List<ExamQuestionDetail>();
    }

    public class ExamQuestionDetail
    {
        public Guid QuestionId { get; set; }
        public string Title { get; set; } 
        public string Context { get; set; } 
        public bool? IsCorrect { get; set; }
        public Guid? MultipleChoiceAnswerId { get; set; } 
        public string StudentAnswerText { get; set; } 
        public int Points { get; set; } 
        public string Feedback { get; set; }
    }
}