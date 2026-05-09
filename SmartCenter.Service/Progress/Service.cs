using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity;

namespace SmartCenter.Service.Progress;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext      = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task MarkLessonCompleteAsync(Request.LessonProgressRequest request)
    {
        var studentId = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(x => x.Type == "studentId")?.Value;
        var studentIdGuid = Guid.Parse(studentId!);

        var lessonExists = await _dbContext.Lessons
            .AnyAsync(l => l.Id == request.LessonId);
        if (!lessonExists)
            throw new KeyNotFoundException($"Không tìm thấy bài học có ID {request.LessonId}.");
        
        var existing = await _dbContext.LearningProcesses
            .FirstOrDefaultAsync(lp => lp.StuId  == studentIdGuid
                                       && lp.LessonId == request.LessonId);

        if (existing is null)
        {
            _dbContext.LearningProcesses.Add(new LearningProcess()
            {
                Id            = Guid.NewGuid(),
                StuId         = studentIdGuid,
                LessonId      = request.LessonId,
                WatchTime     = request.WatchTime,
                IsCompleted   = true,
                LastWatchedAt = DateTimeOffset.UtcNow,
                CreatedAt     = DateTimeOffset.UtcNow,
            });
        }
        else
        {
            // Cộng dồn WatchTime, đánh dấu hoàn thành
            if (request.WatchTime > 0)
                existing.WatchTime += request.WatchTime;
            existing.IsCompleted   = true;
            existing.LastWatchedAt = DateTimeOffset.UtcNow;
            existing.UpdatedAt     = DateTimeOffset.UtcNow;
        }
        await _dbContext.SaveChangesAsync();
    }

    // Tính % hoàn thành = completedLesson / totalLesson
    public async Task<Response.CourseProgressResponse> GetCourseProgressAsync(Guid courseId)
    {
        var studentId = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(x => x.Type == "studentId")?.Value;
        var studentIdGuid = Guid.Parse(studentId!);

        var totalCount = await _dbContext.Lessons
            .CountAsync(l => l.Section.CourseId == courseId);

        if (totalCount == 0)
            return new Response.CourseProgressResponse
            {
                Percent        = 0,
                CompletedCount = 0,
                TotalCount     = 0,
            };

        var completedCount = await _dbContext.LearningProcesses
            .CountAsync(lp => lp.StuId == studentIdGuid 
                              && lp.IsCompleted == true
                              && lp.Lesson.Section.CourseId == courseId);

        var percent = Math.Round((double)completedCount / totalCount * 100, 1);

        return new Response.CourseProgressResponse
        {
            Percent        = percent,
            CompletedCount = completedCount,
            TotalCount     = totalCount,
        };
    }
}