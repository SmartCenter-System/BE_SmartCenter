using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.ExamManagementService;
using SmartCenter.Service.Model;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExamManagementController : ControllerBase
{
    private readonly IService _ExamManagementService;

    public ExamManagementController(IService examManagementService)
    {
        _ExamManagementService = examManagementService;
    }

    [HttpPost(template: "StartExam")]
    [Authorize(Policy = JwtExtensions.StudentPolicy)]
    public async Task<IActionResult> StartExam(Guid ExamId)
    {
        var result = await _ExamManagementService.StartingExam(ExamId);

        return Ok(ApiResponseFactory.SuccessResponse(null, result, HttpContext.TraceIdentifier));
    }

    [HttpPost(template: "SubmitExam")]
    [Authorize(Policy = JwtExtensions.StudentPolicy)]
    public async Task<IActionResult> SubmitExam([FromForm] Request.SubmitExamRequest request)
    {
        return Ok(ApiResponseFactory.SuccessResponse(
            data: await _ExamManagementService.SubmittedExam(request),
            message: "Nộp bài thi thành công.",
            traceId: HttpContext.TraceIdentifier
        ));
    }

    [HttpGet(template: "MyExams")]
    [Authorize(Policy = JwtExtensions.StudentPolicy)]
    public async Task<IActionResult> GetMyExams()
    {
        return Ok(ApiResponseFactory.SuccessResponse(
            data: await _ExamManagementService.GetMyExams(),
            message: "Lấy danh sách bài thi thành công.",
            traceId: HttpContext.TraceIdentifier
        ));
    }


    [HttpGet(template: "{ExamId}/GetExamsByExamId")]
    [Authorize(Policy = JwtExtensions.LecturerPolicy)]
    public async Task<IActionResult> GetExamByExamId(Guid ExamId)
    {
        return Ok(ApiResponseFactory.SuccessResponse(
            data: await _ExamManagementService.GetExamByExamsId(ExamId),
            message: "Lấy thông tin bài thi thành công."
        ));
    }
}