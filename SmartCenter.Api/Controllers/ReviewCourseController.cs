using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.Model;
using SmartCenter.Service.ReviewCourseService;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ReviewCourseController:ControllerBase
{
    private IService _reviewService;
    public ReviewCourseController(IService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost("review-course")]
    [Authorize(Policy = JwtExtensions.StudentPolicy)]
    public async Task<IActionResult> ReviewCoure(Request.CreateReviewRequest request)
    {
        return Ok(ApiResponseFactory.SuccessResponse(await _reviewService.CreateReviewCourseAsync(request),
            "Đánh giá khóa học thành oông", HttpContext.TraceIdentifier));
    }
    
    [HttpGet("get-review-course")]

    public async Task<IActionResult> GetReviewsAsync(Guid courseId, Guid? studentId)
    {
        return Ok(ApiResponseFactory.SuccessResponse(await _reviewService.GetReviewCourseAsync(courseId, studentId),
            "Lấy Review nè", HttpContext.TraceIdentifier));
    }
}