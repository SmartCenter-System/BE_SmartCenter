using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Repository.Entity;
using SmartCenter.Service.JwtService;
using SmartCenter.Service.Lesson;
using SmartCenter.Service.Model;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class LessonController : ControllerBase
{
    private readonly IService _lessonService;

    public LessonController(IService lessonService)
    {
        _lessonService = lessonService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLessons(Guid courseId, Guid sectionId)
    {
        var lesson = await _lessonService.GetLessonsBySectionAsync(sectionId);
        return Ok(ApiResponseFactory.SuccessResponse(lesson, "Get Lesson Details Success", HttpContext.TraceIdentifier));
    }

    [HttpPost]
    [Authorize(Policy = JwtExtensions.AdminOrLecturerPolicy)]
    public async Task<IActionResult> Create(Guid courseId, Guid sectionId,
        [FromBody] Request.CreateLessonRequest request)
    {
        var lesson = await _lessonService.CreateLessonAsync(sectionId, request);
        return Ok(ApiResponseFactory.SuccessResponse(lesson, "Create Lesson Details Success", HttpContext.TraceIdentifier));
    }


    [HttpPut("{lessonId}")]
    [Authorize(Policy = JwtExtensions.AdminOrLecturerPolicy)]
    public async Task<IActionResult> Update(Guid courseId, Guid sectionId, Guid lessonId,
        [FromBody] Request.UpdateLessonRequest request)
    {
        var lesson = await _lessonService.UpdateLessonAsync(lessonId, request);
        return Ok(ApiResponseFactory.SuccessResponse(lesson, "Update Lesson Details Success", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{lessonId}")]
    [Authorize(Policy = JwtExtensions.AdminOrLecturerPolicy)]
    public async Task<IActionResult> Delete(Guid courseId, Guid sectionId, Guid lessonId)
    {
        await _lessonService.DeleteLessonAsync(lessonId);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Delete Lesson Details Success", HttpContext.TraceIdentifier));
    }

    [HttpGet("{lessonId}")]
    [Authorize(Policy = JwtExtensions.AdminOrLecturerPolicy)]
    public async Task<IActionResult> GetLessionById(Guid lessonId)
    {
        await _lessonService.GetLessonAsync(lessonId);
        return Ok(ApiResponseFactory.SuccessResponse(await _lessonService.GetLessonAsync(lessonId), "Success", HttpContext.TraceIdentifier));
    }
}
