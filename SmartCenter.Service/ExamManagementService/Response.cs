using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.ExamManagementService;

public class Response
{
    public class ExamManagementResponse
    {
        public int PointOfStudent { get; set; }
        public int PointOfExam { get; set; }
        public ExamPaperStatus Status { get; set; }
        public string Title { get; set; }
        
        public Guid? StudentId { get; set; }
        
        public String? FirstName { get; set; }

        public String? LastName { get; set; }
    }
}