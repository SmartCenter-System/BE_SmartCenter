using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.ExamManagementService;

public class Response
{
    public class ExamManagementResponse
    {
        public decimal PointOfStudent { get; set; }
        public decimal PointOfExam { get; set; }
        public ExamPaperStatus Status { get; set; }
        public string Title { get; set; }
        
        public Guid? StudentId { get; set; }
        
        public String? FirstName { get; set; }

        public String? LastName { get; set; }
    }
}