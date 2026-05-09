using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.ExamPaper;
using SmartCenter.Service.Model;

namespace SmartCenter.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class ExamPaperController: ControllerBase
{
    private readonly IService _examService;
    public ExamPaperController(IService examService)
    {
        _examService = examService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetExamsByCourse([FromQuery] Guid courseId)
    {
        var result = await _examService.GetExamsByCourseAsync(courseId);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get Exams Success", HttpContext.TraceIdentifier));
    }

    [HttpPost]
    public async Task<IActionResult> CreateExamPaper([FromBody] Request.CreateExamPaperRequest request)
    {
        var result = await _examService.CreateExamPaperAsync(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Create Exams Success", HttpContext.TraceIdentifier));
    }

    [HttpPut("{examId}")]
    public async Task<IActionResult> UpdateExamPaper(Guid examId, [FromBody] Request.UpdateExamPaperRequest request)
    {
        var result = await _examService.UpdateExamPaperAsync(examId, request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Update Exams Success", HttpContext.TraceIdentifier));
    }

    [HttpPost("{examId}/deadline")]
    public async Task<IActionResult> SetDeadline(Guid examId, [FromBody] Request.SetDeadlineRequest request)
    {
        var result = await _examService.SetDeadlineAsync(examId, request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Set Dealine Exams Success", HttpContext.TraceIdentifier));
    }
    
    [HttpDelete("{examId}")]
    public async Task<IActionResult> DeleteExamPaper(Guid examId)
    {
        await _examService.DeleteExamPaperAsync(examId);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Remove Exams Success", HttpContext.TraceIdentifier));
    }

    [HttpPost("add-questions")]
    [Authorize(Policy = JwtExtensions.LecturerPolicy)]
    public async Task<IActionResult> AddQuestionToExam(Guid ExamID,Request.AddMultipleQuestionsRequest request)
    {
        return Ok(ApiResponseFactory.SuccessResponse(await _examService.AddMultipleQuestionsToExamAsync(ExamID,request), "Add Multiple Questions Success", HttpContext.TraceIdentifier));
    }
}