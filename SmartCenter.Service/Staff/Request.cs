using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.Staff;

public class Request
{
    public class ConsultationRequest
    {
        public ConsultReqStatus? Status { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
    
    public class GetEnrollmentsRequest
    {
        public string? SearchName   { get; set; } 
        public Guid?   CourseId { get; set; }
        public int PageIndex    { get; set; } = 1;
        public int PageSize     { get; set; } = 20;
    }
}