namespace SmartCenter.Service.GradeService;

public interface IService
{
    public Task<String> GradeExam(Request.GradeExamRequest request);
    
    public Task<Response.MyExamDetailsResponse> MyExamDetails(Request.MyDetailsRequest request);
}