using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.Model;
using SmartCenter.Service.Progress;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("api/progress")]
[Authorize(Policy = JwtExtensions.StudentPolicy)]
public class ProgressController: ControllerBase
{
    private readonly IService _progressService;

    public ProgressController(IService progressService)
    {
        _progressService = progressService;
    }
    
    [HttpPost("complete")]
    public async Task<IActionResult> MarkComplete(
        [FromBody] Request.LessonProgressRequest request)
    {
        await _progressService.MarkLessonCompleteAsync(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Đánh dấu hoàn thành bài học thành công!", HttpContext.TraceIdentifier));
    }
    
    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetCourseProgress(Guid courseId)
    {
        var result = await _progressService.GetCourseProgressAsync(courseId);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Lấy tiến độ khóa học thành công!", HttpContext.TraceIdentifier));
    }
}