using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;

namespace SmartCenter.Service.Comment;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
 
    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<Response.CommentResponse> AddCommentAsync(Request.AddCommentRequest request)
    {
        var userId = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("Không xác định được người dùng hiện tại.");
        var userIdGuid = Guid.Parse(userId!);
        
        var lessonExists = await _dbContext.Lessons.AnyAsync(l => l.Id == request.LessonId);
        if (!lessonExists)
            throw new KeyNotFoundException($"Không tìm thấy bài học có ID {request.LessonId}.");
 
        int depthLevel = 0;
 
        // Nếu là reply, kiểm tra parent comment
        if (request.ParentCommentId.HasValue)
        {
            var parent = await _dbContext.Comments
                .FirstOrDefaultAsync(c => c.Id == request.ParentCommentId.Value);
 
            if (parent == null)
                throw new KeyNotFoundException("Không tìm thấy comment cha.");
 
            if (parent.LessonId != request.LessonId)
                throw new InvalidOperationException("Comment cha không thuộc bài học này.");
 
            depthLevel = parent.DepthLevel + 1;
        }
 
        var comment = new Repository.Entity.Comment
        {
            Id              = Guid.NewGuid(),
            UserId          = userIdGuid,
            LessonId        = request.LessonId,
            Content         = request.Content,
            ParentCommentId = request.ParentCommentId,
            DepthLevel      = depthLevel,
            IsLocked        = false,
            CreatedAt       = DateTimeOffset.UtcNow,
        };
 
        _dbContext.Comments.Add(comment);
        await _dbContext.SaveChangesAsync();
 
        // Load lại để lấy info User
        await _dbContext.Entry(comment).Reference(c => c.User).LoadAsync();
 
        return MapToResponse(comment);
    }

    public async Task<List<Response.CommentResponse>> GetCommentsByLessonAsync(Guid lessonId)
    {
        var allComments = await _dbContext.Comments
            .Include(c => c.User)
            .Where(c => c.LessonId == lessonId && !c.IsLocked)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
 
        // build cây phân cấp
        var commentDict = allComments.ToDictionary(c => c.Id, MapToResponse);
 
        var roots = new List<Response.CommentResponse>();
 
        foreach (var comment in allComments)
        {
            if (comment.ParentCommentId == null)
            {
                roots.Add(commentDict[comment.Id]);
            }
            else if (commentDict.TryGetValue(comment.ParentCommentId.Value, out var parentResponse))
            {
                parentResponse.Replies.Add(commentDict[comment.Id]);
            }
        }
 
        return roots;
    }

    public async Task DeleteCommentAsync(Guid commentId)
    {
        var userId = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("Không xác định được người dùng hiện tại.");
        var userIdGuid = Guid.Parse(userId!);
 
        var comment = await _dbContext.Comments
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == commentId);
 
        if (comment == null)
            throw new KeyNotFoundException("Không tìm thấy comment.");
        
        if (comment.UserId != userIdGuid) // Chỉ có chủ comment mới được xóa
            throw new UnauthorizedAccessException("Bạn không có quyền xóa comment này.");
 
        bool hasReplies = comment.Replies != null && comment.Replies.Any();
 
        if (hasReplies)
        {
            // Ẩn content, giữ lại comment (để cây reply ko bị vỡ)
            comment.Content  = "[Bình luận đã bị xóa]";
            comment.IsLocked = true;
            comment.UpdatedAt = DateTimeOffset.UtcNow;
            _dbContext.Comments.Update(comment);
        }
        else
        {
            // Ko có reply → xóa hẳn
            _dbContext.Comments.Remove(comment);
        }
 
        await _dbContext.SaveChangesAsync();
    }
    
    private static Response.CommentResponse MapToResponse(Repository.Entity.Comment c) => new()
    {
        Id              = c.Id,
        LessonId        = c.LessonId,
        UserId          = c.UserId,
        UserFullName    = c.User != null ? $"{c.User.FirstName} {c.User.LastName}".Trim() : string.Empty,
        Content         = c.Content,
        ParentCommentId = c.ParentCommentId,
        DepthLevel      = c.DepthLevel,
        IsLocked        = c.IsLocked,
        CreatedAt       = c.CreatedAt,
        Replies         = new List<Response.CommentResponse>(),
    };
}