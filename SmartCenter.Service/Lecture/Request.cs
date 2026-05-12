namespace SmartCenter.Service.Lecture;

public class Request
{
    public class GetSubmittedExamsRequest
    {
        public Guid? CourseId { get; set; }
        public Guid? ExamId   { get; set; }
    }
}