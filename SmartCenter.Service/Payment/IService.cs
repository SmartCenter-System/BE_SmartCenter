
namespace SmartCenter.Service.Payment;

public interface IService
{
    Task<Response.CreatePaymentResponse> CreatePaymentLinkAsync(Guid orderId);
    Task HandleWebhookAsync(Request.SepayWebhookRequest request);
}