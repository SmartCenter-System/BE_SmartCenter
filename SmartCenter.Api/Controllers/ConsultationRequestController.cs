using Microsoft.AspNetCore.Mvc;
using SmartCenter.Service.ConsultationService;

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
        var result = await _consultationService.CreateConsultation(request);
        return Ok(result);
    }
}