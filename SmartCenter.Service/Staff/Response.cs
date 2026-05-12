namespace SmartCenter.Service.Staff;

public class Response
{
    public class ConsultationResponse()
    {
        public int pendingConsultations { get; set; }
        public int newStudentsToday { get; set; }
        public int pendingOrders { get; set; }
    }
        
    public class ConsultationItemResponse
    {
        public Guid Id { get; set; }
        public string FullName  { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public Guid? CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset CreateAt { get; set; }
    }
    
    public class EnrollmentItemResponse
    {
        public Guid   EnrollmentId    { get; set; }
        public Guid   StudentId       { get; set; }
        public string StudentName     { get; set; } = string.Empty;
        public Guid   CourseId        { get; set; }
        public string CourseName      { get; set; } = string.Empty;
        public int    ProgressPercent { get; set; }
        public DateTimeOffset EnrolledAt { get; set; }
    }
}