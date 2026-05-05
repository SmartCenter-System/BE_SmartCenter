using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Repository.Entity;
using SmartCenter.Service.JwtService;
using SmartCenter.Service.Lesson;

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
        => Ok(await _lessonService.GetLessonsBySectionAsync(sectionId));

    [HttpPost]
    [Authorize(Policy = JwtExtensions.AdminOrLecturerPolicy)]
    public async Task<IActionResult> Create(Guid courseId, Guid sectionId, [FromBody] Request.CreateLessonRequest request)
        => Ok(await _lessonService.CreateLessonAsync(sectionId, request));

    [HttpPut("{lessonId}")]
    [Authorize(Policy = JwtExtensions.AdminOrLecturerPolicy)]
    public async Task<IActionResult> Update(Guid courseId, Guid sectionId, Guid lessonId, [FromBody] Request.UpdateLessonRequest request)
        => Ok(await _lessonService.UpdateLessonAsync(lessonId, request));

    [HttpDelete("{lessonId}")]
    [Authorize(Policy = JwtExtensions.AdminOrLecturerPolicy)]
    public async Task<IActionResult> Delete(Guid courseId, Guid sectionId, Guid lessonId)
    {
        await _lessonService.DeleteLessonAsync(lessonId);
        return Ok(new { message = "Xóa bài học thành công." });
    }
}
