namespace SmartCenter.Service.ConsultationService;

public class Request
{
    public class ConsultationRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Guid CourseId { get; set; }
        
        public DateTimeOffset RequestDate { get; set; }
        
        public string Status { get; set; }
        public Guid? UserId { get; set; } = null;
        public string? Note { get; set; } = null;
        public string? Message { get; set; } = null;
    }
}