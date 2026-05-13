namespace SmartCenter.Service.Lecture;

public class Response
{
    public class SubmittedExamResponse
    {
        public Guid   SubmissionId  { get; set; }
        public Guid   StudentId     { get; set; }
        public string StudentName   { get; set; } = string.Empty;
        public Guid   ExamId        { get; set; }
        public string ExamTitle     { get; set; } = string.Empty;
        public DateTimeOffset SubmittedAt { get; set; }
        public string GradingStatus { get; set; } = string.Empty; // UNGRADED | GRADED
    }
}