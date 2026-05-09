namespace SmartCenter.Service.Comment;

public interface IService
{
    Task<Response.CommentResponse> AddCommentAsync(Request.AddCommentRequest request);
    Task<List<Response.CommentResponse>> GetCommentsByLessonAsync(Guid lessonId);
    Task DeleteCommentAsync(Guid commentId);
}