namespace SmartCenter.Service.Financial;

public class Request
{
    public class FinancialFilterRequest
    {
        public string? Status { get; set; }
        public Guid? StudentId { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }
    
    public class RevenuePeriodRequest
    {
        public required string Period { get; set; }
        
        public DateTimeOffset? FromDate { get; set; }
        
        public DateTimeOffset? ToDate { get; set; }
    }
}