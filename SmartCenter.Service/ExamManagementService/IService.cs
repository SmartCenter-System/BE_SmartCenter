namespace SmartCenter.Service.ExamManagementService;

public interface IService
{
    public Task<String> StartingExam(Guid ExamID);
    
    public Task<String> SubmittedExam(Request.SubmitExamRequest request);
    
    public Task<List<Response.ExamManagementResponse>> GetMyExams();
    
    public Task<List<Response.ExamManagementResponse>> GetExamByExamsId(Guid ExamID);
}