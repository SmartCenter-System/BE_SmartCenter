using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;

namespace SmartCenter.Service.Section;

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

    private async Task AuthorizeSectionAsync(Guid sectionId)
    {
        if (IsAdmin()) return;
        var lecturerId = GetLecturerId();
        var owns = await _dbContext.Sections
            .AnyAsync(s => s.Id == sectionId && s.Course.LecId == lecturerId);
        if (!owns)
            throw new UnauthorizedAccessException("Bạn không có quyền thao tác với section này.");
    }

    public async Task<List<Response.SectionResponse>> GetSectionsByCourseAsync(Guid courseId)
    {
        return await _dbContext.Sections
            .Where(s => s.CourseId == courseId)
            .OrderBy(s => s.Position)
            .Select(s => new Response.SectionResponse
            {
                Id    = s.Id,
                Title = s.Title,
                Order = s.Position,
            })
            .ToListAsync();
    }

    public async Task<Response.SectionResponse> CreateSectionAsync(Guid courseId, Request.CreateSectionRequest request)
    {
        await AuthorizeCourseAsync(courseId);

        var section = new Repository.Entity.Section
        {
            Id       = Guid.NewGuid(),
            CourseId = courseId,
            Title    = request.Title,
            Position    = request.Position,
        };

        var sectionNew = new Response.SectionResponse
        {
            Id = section.Id,
            Title = section.Title,
            Order = section.Position
        };
        
        _dbContext.Sections.Add(section);
        await _dbContext.SaveChangesAsync();

        return sectionNew;
    }

    public async Task<Response.SectionResponse> UpdateSectionAsync(Guid sectionId, Request.UpdateSectionRequest request)
    {
        await AuthorizeSectionAsync(sectionId);

        var section = await _dbContext.Sections.FindAsync(sectionId)
            ?? throw new Exception("Không tìm thấy section.");

        if (request.Title != null) section.Title = request.Title;
        if (request.Position != null) section.Position = request.Position.Value;

        
        var sectionNew = new Response.SectionResponse
        {
            Id = section.Id,
            Title = section.Title,
            Order = section.Position
        };
        await _dbContext.SaveChangesAsync();
        return sectionNew;
    }

    public async Task DeleteSectionAsync(Guid sectionId)
    {
        await AuthorizeSectionAsync(sectionId);

        var section = await _dbContext.Sections.FindAsync(sectionId)
            ?? throw new Exception("Không tìm thấy section.");

        _dbContext.Sections.Remove(section);
        await _dbContext.SaveChangesAsync();
    }
}