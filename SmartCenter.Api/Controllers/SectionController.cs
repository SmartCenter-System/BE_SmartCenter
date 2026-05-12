using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.Model;
using SmartCenter.Service.Section;

namespace SmartCenter.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class SectionController: ControllerBase
{
    private readonly IService _sectionService;

    public SectionController(IService sectionService)
    {
        _sectionService = sectionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSections(Guid courseId)
    {
        var section = await _sectionService.GetSectionsByCourseAsync(courseId);
        return Ok(ApiResponseFactory.SuccessResponse(section, "Get Section Details Success", HttpContext.TraceIdentifier));
    }


    [HttpPost]
    [Authorize(Policy = JwtExtensions.AdminOrLecturerPolicy)]
    public async Task<IActionResult> Create(Guid courseId, [FromBody] Request.CreateSectionRequest request)
    {
        var section = await _sectionService.CreateSectionAsync(courseId, request);
        return Ok(ApiResponseFactory.SuccessResponse(section, "Create Section Details Success", HttpContext.TraceIdentifier));
    }

    [HttpPut("{sectionId}")]
    [Authorize(Policy = JwtExtensions.AdminOrLecturerPolicy)]
    public async Task<IActionResult> Update(Guid courseId, Guid sectionId,
        [FromBody] Request.UpdateSectionRequest request)
    {
        var section = await _sectionService.UpdateSectionAsync(sectionId, request);
        return Ok(ApiResponseFactory.SuccessResponse(section, "Get Section Details Success", HttpContext.TraceIdentifier));
    }


    [HttpDelete("{sectionId}")]
    [Authorize(Policy = JwtExtensions.AdminOrLecturerPolicy)]
    public async Task<IActionResult> Delete(Guid courseId, Guid sectionId)
    {
        await _sectionService.DeleteSectionAsync(sectionId);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Delete Section Details Success", HttpContext.TraceIdentifier));
    }
}