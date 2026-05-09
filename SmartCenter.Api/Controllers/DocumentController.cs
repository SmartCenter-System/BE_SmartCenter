using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.Document;
using SmartCenter.Service.Model;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize(Policy = JwtExtensions.LecturerPolicy)]
public class DocumentController: ControllerBase
{
    private readonly IService _documentService;
 
    public DocumentController(IService documentService)
    {
        _documentService = documentService;
    }
    
    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(
        [FromQuery] Guid lessonId,
        [FromForm] Request.UploadDocumentRequest request)
    {
        var result = await _documentService.UploadDocumentAsync(lessonId, request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Upload tài liệu thành công!", HttpContext.TraceIdentifier));
    }
    
    [HttpGet("lesson/{lessonId}")]
    public async Task<IActionResult> GetDocumentsByLesson(Guid lessonId)
    {
        var result = await _documentService.GetDocumentsByLessonAsync(lessonId);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Lấy danh sách tài liệu thành công!", HttpContext.TraceIdentifier));
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        await _documentService.DeleteDocumentAsync(id);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Xóa tài liệu thành công!", HttpContext.TraceIdentifier));
    }

}