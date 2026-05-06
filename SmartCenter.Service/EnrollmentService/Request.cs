namespace SmartCenter.Service.EnrollmentService;

public class Request
{
    public class EnrollmentRequest
    {
        public Guid CourseId { get; set; }
        public Guid TransactionId { get; set; }
    }
}