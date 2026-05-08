using Microsoft.AspNetCore.Mvc;
using SmartCenter.Service.ConsultationService;
using SmartCenter.Service.Model;

namespace SmartCenter.Controllers;

[ApiController]
[Route("[controller]")]
public class ConsultationRequestController:ControllerBase
{
    private readonly IService _consultationService;

    public ConsultationRequestController(IService consultationService)
    {
        _consultationService = consultationService;
    }

    [HttpPost("CreateConsultationRequest")]
    public async Task<IActionResult> CreateConsultation([FromForm] Request.ConsultationRequest request)
    {
        return Ok(ApiResponseFactory.SuccessResponse(
            data: await _consultationService.CreateConsultation(request),
            message: "Tạo yêu cầu tư vấn thành công."
        ));
    }
}