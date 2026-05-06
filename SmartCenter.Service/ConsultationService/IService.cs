namespace SmartCenter.Service.ConsultationService;

public interface IService
{
    public Task<string> CreateConsultation(Request.ConsultationRequest request);
}