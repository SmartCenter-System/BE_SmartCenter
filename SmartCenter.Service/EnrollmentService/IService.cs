namespace SmartCenter.Service.EnrollmentService;

public interface IService
{
    public Task<String> CreateEnrollment(Request.EnrollmentRequest request);
    
    public Task<List<Response.MyEnrollmentResponse>> GetMyEnrollment();
}