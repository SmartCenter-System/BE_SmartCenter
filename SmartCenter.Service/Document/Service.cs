using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;

namespace SmartCenter.Service.Document;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    private readonly MediaService.IService _mediaService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext dbContext, MediaService.IService mediaService, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _mediaService = mediaService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response.DocumentResponse> UploadDocumentAsync(Guid lessonId, Request.UploadDocumentRequest request)
    {
        await AuthorizeLessonAsync(lessonId);
 
        // Upload file lên Cloudinary
        var (fileUrl, publicId) = await _mediaService.UploadFileAsync(request.File);
        
        //    PublicId lưu vào FileName để dùng khi xóa
        var document = new Repository.Entity.Document
        {
            Id        = Guid.NewGuid(),
            LessonId  = lessonId,
            FileName  = publicId,                      
            FileUrl   = fileUrl,
            FileType  = request.File.ContentType,        // VD: "application/pdf"
            CreatedAt = DateTimeOffset.UtcNow,
        };
 
        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync();
        return MapToResponse(document);
    }
    
    private Guid GetLecturerId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
                        .FindFirst("lecturerId")?.Value
                    ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin giảng viên.");
        return Guid.Parse(claim);
    }
    
    private async Task AuthorizeLessonAsync(Guid lessonId)
    {
        var lecturerId = GetLecturerId();
        var courseLecId = await _dbContext.Lessons
            .Where(l => l.Id == lessonId)
            .Select(l => (Guid?)l.Section.Course.LecId)
            .FirstOrDefaultAsync();
        if (courseLecId == null)
            throw new KeyNotFoundException($"Không tìm thấy bài học có ID {lessonId}.");
        
        if (courseLecId != lecturerId)
            throw new UnauthorizedAccessException("Bạn không có quyền thao tác với bài học này.");
    }
    
    private static Response.DocumentResponse MapToResponse(Repository.Entity.Document d) => new()
    {
        DocumentId = d.Id,
        LessonId   = d.LessonId,
        FileName   = d.FileName,
        FileUrl    = d.FileUrl,
        FileType   = d.FileType,
        CreatedAt  = d.CreatedAt,
    };
    
    public async Task<List<Response.DocumentResponse>> GetDocumentsByLessonAsync(Guid lessonId)
    {
        return await _dbContext.Documents
            .Where(d => d.LessonId == lessonId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new Response.DocumentResponse
            {
                DocumentId = d.Id,
                LessonId   = d.LessonId,
                FileName   = d.FileName,
                FileUrl    = d.FileUrl,
                FileType   = d.FileType,
                CreatedAt  = d.CreatedAt,
            })
            .ToListAsync();
    }

    public async Task DeleteDocumentAsync(Guid documentId)
    {
        var document = await _dbContext.Documents
            .Include(d => d.Lesson)
            .FirstOrDefaultAsync(d => d.Id == documentId);
 
        if (document == null)
            throw new KeyNotFoundException("Không tìm thấy tài liệu.");
        
        await AuthorizeLessonAsync(document.LessonId);
        
        await _mediaService.DeleteFileAsync(document.FileName);

        _dbContext.Documents.Remove(document);
        await _dbContext.SaveChangesAsync();
    }
}