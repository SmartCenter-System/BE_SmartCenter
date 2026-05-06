namespace SmartCenter.Service.ExamPaper;

public interface IService
{
    Task<List<Response.ExamResponse>> GetExamsByCourseAsync(Guid courseId);
    Task<Response.ExamResponse> CreateExamPaperAsync(Request.CreateExamPaperRequest request);
    Task<Response.ExamResponse> UpdateExamPaperAsync(Guid examId, Request.UpdateExamPaperRequest request);
    Task<Response.DeadlineResponse> SetDeadlineAsync(Guid examId, Request.SetDeadlineRequest request);  
    Task DeleteExamPaperAsync(Guid examId);
}