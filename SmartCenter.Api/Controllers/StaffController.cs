using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.Model;
using SmartCenter.Service.Staff;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("api/staff")]
public class StaffController : ControllerBase
{
    private readonly IService _staffService;

    public StaffController(IService staffService)
    {
        _staffService = staffService;
    }

    [HttpGet("dashboard/stats")]
    [Authorize(Policy = JwtExtensions.StaffPolicy)]
    public async Task<IActionResult> GetDashboardStats()
    {
        return Ok(ApiResponseFactory.SuccessResponse(await _staffService.GetConsultations(), "Thành công",
            HttpContext.TraceIdentifier));
    }

    [HttpPost("accept-consultation")]
    [Authorize(Policy = JwtExtensions.StaffPolicy)]
    public async Task<IActionResult> AcceptConsultation(Guid consultationId)
    {
        return Ok(ApiResponseFactory.SuccessResponse(await _staffService.AcceptConsultation(consultationId),
            "Nhận đơn thành công", HttpContext.TraceIdentifier));
    }
    
    [HttpPost("reject-consultation")]
    [Authorize(Policy = JwtExtensions.StaffPolicy)]
    public async Task<IActionResult> RejectConsultation(Guid consultationId)
    {
        return Ok(ApiResponseFactory.SuccessResponse(await _staffService.RejectConsultation(consultationId),
            "Đã từ chối", HttpContext.TraceIdentifier));
    }
}