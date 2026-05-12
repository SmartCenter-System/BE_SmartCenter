using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.Lecture;
using SmartCenter.Service.Model;

namespace SmartCenter.Api.Controllers;
[ApiController]
[Route("api")]
[Authorize(Policy = JwtExtensions.AdminOrLecturerPolicy)]
public class LecturerController: ControllerBase
{
    private readonly IService _lecService;

    public LecturerController(IService lecService)
    {
        _lecService = lecService;
    }

    [HttpGet("GradeExam/submitted-exams")]
    public async Task<IActionResult> GetSubmittedExams([FromQuery] Request.GetSubmittedExamsRequest request)
    {
        var result = await _lecService.GetSubmittedExamsAsync(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get submitted exams success", HttpContext.TraceIdentifier));
    }
    
}