using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.Course;
using SmartCenter.Service.Model;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController: ControllerBase
{
    private readonly IService _courseSevice;

    public CoursesController(IService courseSevice)
    {
        _courseSevice = courseSevice;
    }

    [HttpGet]
    public async Task<IActionResult> GetCourses([FromQuery] Request.CourseFilterRequest request)
    {
        var courseFilter = await _courseSevice.GetCoursesAsync(request);
        return Ok(ApiResponseFactory.SuccessResponse(courseFilter, "Get Courses Success", HttpContext.TraceIdentifier));
    }

    [HttpGet("{courseId}")]
    public async Task<IActionResult> GetCourseDetail(Guid courseId)
    {
        var courseDetail = await _courseSevice.GetCourseDetailAsync(courseId);
        return Ok(ApiResponseFactory.SuccessResponse(courseDetail, "Get Course Details Success", HttpContext.TraceIdentifier));
    }
    
    [Authorize(Policy = JwtExtensions.UserPolicy)]
    [HttpGet("{courseId}/previews")]
    public async Task<IActionResult> GetCoursePreview(Guid courseId)
    {
        var coursePreview = await _courseSevice.GetCoursePreviewAsync(courseId);
        return Ok(ApiResponseFactory.SuccessResponse(coursePreview, "Get Course Preview Success", HttpContext.TraceIdentifier));
    }
}