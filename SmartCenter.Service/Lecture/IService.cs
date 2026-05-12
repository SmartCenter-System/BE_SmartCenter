namespace SmartCenter.Service.Lecture;

public interface IService
{
    Task<List<Response.SubmittedExamResponse>> GetSubmittedExamsAsync(Request.GetSubmittedExamsRequest request);
}