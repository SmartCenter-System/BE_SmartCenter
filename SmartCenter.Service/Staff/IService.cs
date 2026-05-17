using SmartCenter.Service.Base;

namespace SmartCenter.Service.Staff;

public interface IService
{
    Task<Response.ConsultationResponse> GetConsultations();
    
    Task<String> AcceptConsultation(Guid ConsultationId);

    Task<string> RejectConsultation(Guid ConsultationId);
    
    Task<String> ProcessingConsultation(Guid ConsultationId);
    
    Task<PagedResult<Response.ConsultationItemResponse>> GetConsultationsAsync(Request.ConsultationRequest request);
    
    Task<PagedResult<Response.EnrollmentItemResponse>> GetEnrollmentsAsync(Request.GetEnrollmentsRequest request);
}