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

    [HttpPost]
    [Authorize(Policy = JwtExtensions.AdminOrLecturerPolicy)]
    public async Task<IActionResult> CreateCourse([FromBody] Request.CreateCourseRequest request)
    {
        var course = await _courseSevice.CreateCourseAsync(request);
        return Ok(ApiResponseFactory.SuccessResponse(course, "Create Course Success", HttpContext.TraceIdentifier));
    }

    [HttpPut("{courseId}")]
    [Authorize(Policy = JwtExtensions.AdminOrLecturerPolicy)]
    public async Task<IActionResult> UpdateCourse(Guid courseId, [FromBody] Request.UpdateCourseRequest request)
    {
        var updateCourse = await _courseSevice.UpdateCourseAsync(courseId, request);
        return Ok(ApiResponseFactory.SuccessResponse(updateCourse, "Update Course Success", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{courseId}")]
    [Authorize(Policy = JwtExtensions.AdminOrLecturerPolicy)]
    public async Task<IActionResult> DeleteCourse(Guid courseId)
    {
        await  _courseSevice.DeleteCourseAsync(courseId);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Delete Course Success", HttpContext.TraceIdentifier));
    }
}