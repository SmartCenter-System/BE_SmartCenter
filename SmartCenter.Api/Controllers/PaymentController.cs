using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.Course;
using SmartCenter.Service.Model;
using IService = SmartCenter.Service.Payment.IService;
using Request = SmartCenter.Service.Payment.Request;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController: ControllerBase
{
    private readonly IService _paymentService;
 
    public PaymentController(IService paymentService)
    {
        _paymentService = paymentService;
    }
    
    // [Authorize(Policy = JwtExtensions.StudentPolicy)]
    [HttpPost("create-link")]
    public async Task<IActionResult> CreatePaymentLink([FromBody] Request.CreatePaymentRequest request)
    {
        var result = await _paymentService.CreatePaymentLinkAsync(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Tạo link thanh toán thành công!", HttpContext.TraceIdentifier));
    }
    
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] Request.SepayWebhookRequest request)
    {
        await _paymentService.HandleWebhookAsync(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Webhook processed", HttpContext.TraceIdentifier));
    }
}