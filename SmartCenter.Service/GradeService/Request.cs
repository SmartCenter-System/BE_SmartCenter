namespace SmartCenter.Service.GradeService;

public class Request
{
    public class GradeExamRequest
    {
        public Guid ExamId { get; set; }

        public Guid StudentId { get; set; }
        
        public List<GradeDetailRequest> GradeDetails { get; set; } = new List<GradeDetailRequest>();
    }

    public class GradeDetailRequest
    {
        public Guid ExamManagementDetailId { get; set; }

        public int? Point { get; set; }

        public string? Feedback { get; set; }
    }
    
    public class MyDetailsRequest
    {
        public Guid ExamId { get; set; }
    }
}