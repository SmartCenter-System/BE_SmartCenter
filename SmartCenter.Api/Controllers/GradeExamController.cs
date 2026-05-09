using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.GradeService;
using SmartCenter.Service.Model;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class GradeExamController:ControllerBase
{
    private readonly IService _gradeService;

    public GradeExamController(IService gradeService)
    {
        _gradeService = gradeService;
    }

    [HttpPost]
    [Authorize(Policy = JwtExtensions.LecturerPolicy)]
    public async Task<IActionResult> GradeExam([FromBody] Request.GradeExamRequest request)
    {
        return Ok(ApiResponseFactory.SuccessResponse(
            data: await _gradeService.GradeExam(request),
            message: "Hoàn tất yêu cầu chấm điểm bài thi.",
            traceId: HttpContext.TraceIdentifier
        ));
    }

    [HttpGet(template: "my-exam-details")]
    [Authorize(Policy = JwtExtensions.StudentPolicy)]
    public async Task<IActionResult> GetMyExamDetails([FromQuery] Request.MyDetailsRequest request)
    {
        return Ok(ApiResponseFactory.SuccessResponse(
            data: await _gradeService.MyExamDetails(request),
            message: "Lấy chi tiết kết quả bài thi thành công.",
            traceId: HttpContext.TraceIdentifier
        ));
    }
        
}