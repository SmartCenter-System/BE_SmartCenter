namespace SmartCenter.Service.Staff;

public interface IService
{
    Task<Response.ConsultationResponse> GetConsultations();
    
    Task<String> AcceptConsultation(Guid ConsultationId);

    Task<string> RejectConsultation(Guid ConsultationId);
}