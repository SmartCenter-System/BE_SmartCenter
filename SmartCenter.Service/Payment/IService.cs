
namespace SmartCenter.Service.Payment;

public interface IService
{
    Task<Response.CreatePaymentResponse> CreatePaymentLinkAsync(Request.CreatePaymentRequest request);
    Task HandleWebhookAsync(Request.SepayWebhookRequest request);
}