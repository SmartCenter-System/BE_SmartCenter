using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;

namespace SmartCenter.Service.Lesson;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetLecturerId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
            .FindFirst("lecturerId")?.Value
            ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin giảng viên.");
        return Guid.Parse(claim);
    }

    private bool IsAdmin() =>
        _httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;

    private async Task AuthorizeLessonAsync(Guid lessonId)
    {
        if (IsAdmin()) return;
        var lecturerId = GetLecturerId();
        var owns = await _dbContext.Lessons
            .AnyAsync(l => l.Id == lessonId && l.Section.Course.LecId == lecturerId);
        if (!owns)
            throw new UnauthorizedAccessException("Bạn không có quyền thao tác với bài học này.");
    }

    private async Task AuthorizeSectionAsync(Guid sectionId)
    {
        if (IsAdmin()) return;
        var lecturerId = GetLecturerId();
        var owns = await _dbContext.Sections
            .AnyAsync(s => s.Id == sectionId && s.Course.LecId == lecturerId);
        if (!owns)
            throw new UnauthorizedAccessException("Bạn không có quyền thao tác với section này.");
    }

    public async Task<List<Response.LessonResponse>> GetLessonsBySectionAsync(Guid sectionId)
    {
        return await _dbContext.Lessons
            .Where(l => l.SectionId == sectionId)
            .OrderBy(l => l.Position)
            .Select(l => new Response.LessonResponse
            {
                Id          = l.Id,
                Title       = l.Title,
                Description = l.Description,
                VideoUrl    = l.VideoUrl,
                Order       = l.Position,
                IsPreview   = l.IsPreview,
                Duration    = l.Duration,
            })
            .ToListAsync();
    }

    public async Task<Response.LessonResponse> CreateLessonAsync(Guid sectionId, Request.CreateLessonRequest request)
    {
        await AuthorizeSectionAsync(sectionId);

        var lesson = new Repository.Entity.Lesson
        {
            Id = Guid.NewGuid(),
            SectionId = sectionId,
            Title = request.Title,
            Description = request.Description,
            VideoUrl = request.VideoUrl,
            Position = request.Order,
            IsPreview = request.IsPreview,
            CreatedAt = DateTimeOffset.UtcNow,
            Duration = request.Duration,
        };

        _dbContext.Lessons.Add(lesson);
        await _dbContext.SaveChangesAsync();

        return new Response.LessonResponse
        {
            Id          = lesson.Id,
            Title       = lesson.Title,
            Description = lesson.Description,
            VideoUrl    = lesson.VideoUrl,
            Order       = lesson.Position,
            IsPreview   = lesson.IsPreview,
            Duration    =  lesson.Duration,
        };
    }

    public async Task<Response.LessonResponse> UpdateLessonAsync(Guid lessonId, Request.UpdateLessonRequest request)
    {
        await AuthorizeLessonAsync(lessonId);

        var lesson = await _dbContext.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId)
                     ?? throw new Exception("Không tìm thấy bài học.");

        if (request.Title       != null) lesson.Title       = request.Title;
        if (request.Description != null) lesson.Description = request.Description;
        if (request.VideoUrl    != null) lesson.VideoUrl    = request.VideoUrl;
        if (request.Order       != null) lesson.Position       = request.Order.Value;
        if (request.IsPreview   != null) lesson.IsPreview   = request.IsPreview.Value;
        if (request.Duration     != null) lesson.Duration    = request.Duration.Value;
        lesson.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return new Response.LessonResponse
        {
            Id          = lesson.Id,
            Title       = lesson.Title,
            Description = lesson.Description,
            VideoUrl    = lesson.VideoUrl,
            Order       = lesson.Position,
            IsPreview   = lesson.IsPreview,
            Duration    =  lesson.Duration,
        };
    }

    public async Task DeleteLessonAsync(Guid lessonId)
    {
        await AuthorizeLessonAsync(lessonId);

        var lesson = await _dbContext.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId)
                     ?? throw new Exception("Không tìm thấy bài học.");

        _dbContext.Lessons.Remove(lesson);
        await _dbContext.SaveChangesAsync();
    }
}