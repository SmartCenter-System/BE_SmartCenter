using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity.Enums;
using SmartCenter.Service.Base;

namespace SmartCenter.Service.Course;

public class Service: IService
{
    
    private readonly AppDbContext _dbContext;
    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
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
}