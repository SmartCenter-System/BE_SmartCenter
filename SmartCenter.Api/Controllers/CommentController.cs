using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Service.Comment;
using SmartCenter.Service.Model;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("api/comments")]
[Authorize]
public class CommentController: ControllerBase
{
    private readonly IService _commentService;
 
    public CommentController(IService commentService)
    {
        _commentService = commentService;
    }
    
    [HttpPost]
    public async Task<IActionResult> AddComment([FromBody] Request.AddCommentRequest request)
    {
        var result = await _commentService.AddCommentAsync(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Thêm bình luận thành công!", HttpContext.TraceIdentifier));
    }
    
    [HttpGet("lesson/{lessonId}")]
    public async Task<IActionResult> GetCommentsByLesson(Guid lessonId)
    {
        var result = await _commentService.GetCommentsByLessonAsync(lessonId);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Lấy danh sách bình luận thành công!", HttpContext.TraceIdentifier));
    }
    
    /// Xóa comment: IsLocked = true, ẩn content nếu có reply.
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        await _commentService.DeleteCommentAsync(id);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Xóa bình luận thành công!", HttpContext.TraceIdentifier));
    }
}