using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.ExamManagementService;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExamManagementController:ControllerBase
{
    private readonly IService _ExamManagementService;

    public ExamManagementController(IService examManagementService)
    {
        _ExamManagementService = examManagementService;
    }

    [HttpPost(template:"StartExam")]
    [Authorize(Policy = JwtExtensions.StudentPolicy)]
    public async Task<IActionResult> StartExam(Guid ExamId)
    {
        var result = await _ExamManagementService.StartingExam(ExamId);
        return Ok(result);
    }

    [HttpPost(template: "SubmitExam")]
    [Authorize(Policy = JwtExtensions.StudentPolicy)]
    public async Task<IActionResult> SubmitExam([FromForm]Request.SubmitExamRequest request)
    {
        var result = await _ExamManagementService.SubmittedExam(request);
        return Ok(result);
    }

    [HttpGet(template: "MyExams")]
    [Authorize(Policy = JwtExtensions.StudentPolicy)]
    public async Task<IActionResult> GetMyExams()
    {
        var result = await _ExamManagementService.GetMyExams();
        return Ok(result);
    }

    [HttpGet(template: "{ExamId}/GetExamsByExamId")]
    [Authorize(Policy = JwtExtensions.LecturerPolicy)]
    public async Task<IActionResult> GetExamByExamId(Guid ExamId)
    {
        var result = await _ExamManagementService.GetExamByExamsId(ExamId);
        return Ok(result);
    }
    
}