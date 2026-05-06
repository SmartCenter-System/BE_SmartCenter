using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity.Enums;
using SmartCenter.Service.Base;

namespace SmartCenter.Service.Course;

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

    private async Task AuthorizeCourseAsync(Guid courseId)
    {
        if (IsAdmin()) return;
        var lecturerId = GetLecturerId();
        var owns = await _dbContext.Courses
            .AnyAsync(c => c.Id == courseId && c.LecId == lecturerId);
        if (!owns)
            throw new UnauthorizedAccessException("Bạn không có quyền thao tác với khóa học này.");
    }

    
    public async Task<PagedResult<Response.CourseItemResponse>> GetCoursesAsync(Request.CourseFilterRequest request)
    {
        var query = _dbContext.Courses.AsQueryable();

        if (request.CategoryId.HasValue)
            query = query.Where(x => x.CourseCategories.Any(c => c.CategoryId == request.CategoryId));

        if (request.MinPrice.HasValue)
            query = query.Where(x => x.BasePrice >= request.MinPrice);

        if (request.MaxPrice.HasValue)
            query = query.Where(x => x.BasePrice <= request.MaxPrice);

        if (request.Mode.HasValue)
            query = query.Where(x => x.CourseType == request.Mode);

        if (request.Keyword != null)
            query = query.Where(x => x.Description.Contains(request.Keyword));
        
        var total = await query.CountAsync();
        
        var items = await query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new Response.CourseItemResponse
            {
                Id = x.Id,
                Title = x.CourseName,
                Price = x.BasePrice,
                Mode = x.CourseType,
                AvailableSlots = x.MaxStudents - _dbContext.Enrollments.Count(e => e.CourseId == x.Id && e.Status == EnrollmentStatus.Paid)
            })
            .ToListAsync();

        return new PagedResult<Response.CourseItemResponse>()
        {
            Items = items,
            Total = total,
        };
    }

    public async Task<Response.CourseDetailResponse?> GetCourseDetailAsync(Guid courseId)
    {
        var query = _dbContext.Courses
            .Where(x => x.Id == courseId)
            .Select(c => new Response.CourseDetailResponse
            {
                Id = c.Id,
                Title = c.CourseName,
                Price = c.BasePrice,
                Mode = c.CourseType,
                AvailableSlots = c.MaxStudents -
                                 _dbContext.Enrollments.Count(e =>
                                     e.CourseId == c.Id && e.Status == EnrollmentStatus.Paid),

                Sections = c.Sections.Select(s => new Response.SectionResponse()
                {
                    Id = s.Id,
                    Title = s.Title,
                    Lessons = s.Lessons.Select(l => new Response.LessonResponse()
                    {
                        Id = l.Id,
                        Title = l.Title,
                        IsPreview = l.IsPreview,
                    }).ToList()
                }).ToList()
            });
        var courseDetail = await query.FirstOrDefaultAsync();

        return courseDetail;

    }

    public async Task<List<Response.LessonResponse>> GetCoursePreviewAsync(Guid courseId)
    {
        var query = _dbContext.Lessons
            .Where(l => l.CourseId == courseId && l.IsPreview)
            .Select(l => new Response.LessonResponse()
            {
                Id = l.Id,
                Title = l.Title,
                IsPreview = l.IsPreview    
            });
        var lessons = await query.ToListAsync();
        return lessons;
    }

    public async Task<Response.CourseDetailResponse> CreateCourseAsync(Request.CreateCourseRequest request)
    {
        var lecturerId = IsAdmin()? Guid.Empty : GetLecturerId();

        var course = new Repository.Entity.Course()
        {
            Id = Guid.NewGuid(),
            LecId = lecturerId,
            CourseName = request.CourseName,
            Description = request.Description,
            BasePrice = request.BasePrice,
            ImgUrl = request.ImgUrl,
            CourseType = request.CourseType,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            MaxStudents = request.MaxStudents,
            AcademicYear = request.AcademicYear,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        
        _dbContext.Courses.Add(course);
        await _dbContext.SaveChangesAsync();
        
        return (await GetCourseDetailAsync(course.Id))!;
    }

    public async Task<Response.CourseDetailResponse> UpdateCourseAsync(Guid courseId, Request.UpdateCourseRequest request)
    {
        await AuthorizeCourseAsync(courseId);
        var course = await _dbContext.Courses.FindAsync(courseId)
                     ?? throw new Exception("Course not found.");
        
        if (request.CourseName  != null) course.CourseName  = request.CourseName;
        if (request.Description != null) course.Description = request.Description;
        if (request.BasePrice   != null) course.BasePrice   = request.BasePrice.Value;
        if (request.ImgUrl      != null) course.ImgUrl      = request.ImgUrl;
        if (request.StartAt     != null) course.StartAt     = request.StartAt.Value;
        if (request.EndAt       != null) course.EndAt       = request.EndAt.Value;
        if (request.MaxStudents != null) course.MaxStudents = request.MaxStudents.Value;
        if (request.IsActive    != null) course.IsActive    = request.IsActive.Value;
        course.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
        return (await GetCourseDetailAsync(courseId))!;
    }

    public async Task DeleteCourseAsync(Guid courseId)
    {
        await AuthorizeCourseAsync(courseId);

        var course = await _dbContext.Courses.FindAsync(courseId);
        if (course == null)
            throw new Exception("Course not found.");

        _dbContext.Courses.Remove(course);
        await _dbContext.SaveChangesAsync();
    }
}