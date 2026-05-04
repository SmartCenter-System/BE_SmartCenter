using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Service.Section;

namespace SmartCenter.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class SectionController: ControllerBase
{
    private readonly IService _sectionService;

    public SectionController(IService sectionService)
    {
        _sectionService = sectionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSections(Guid courseId)
        => Ok(await _sectionService.GetSectionsByCourseAsync(courseId));

    [HttpPost]
    [Authorize(Policy = "AdminOrLecturer")]
    public async Task<IActionResult> Create(Guid courseId, [FromBody] Request.CreateSectionRequest request)
        => Ok(await _sectionService.CreateSectionAsync(courseId, request));

    [HttpPut("{sectionId}")]
    [Authorize(Policy = "AdminOrLecturer")]
    public async Task<IActionResult> Update(Guid courseId, Guid sectionId, [FromBody] Request.UpdateSectionRequest request)
        => Ok(await _sectionService.UpdateSectionAsync(sectionId, request));

    [HttpDelete("{sectionId}")]
    [Authorize(Policy = "AdminOrLecturer")]
    public async Task<IActionResult> Delete(Guid courseId, Guid sectionId)
    {
        await _sectionService.DeleteSectionAsync(sectionId);
        return Ok(new { message = "Xóa section thành công." });
    }
}