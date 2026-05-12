using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;

namespace SmartCenter.Service.Lecture;

public class Service : IService
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
    
    public async Task<List<Response.SubmittedExamResponse>> GetSubmittedExamsAsync(Request.GetSubmittedExamsRequest request)
    {
        var lecturerId = GetLecturerId();

        var query = _dbContext.ExamManagements
            .Include(em => em.ExamPaper)
            .ThenInclude(ep => ep.Lesson)
            .Include(em => em.Student)
            .ThenInclude(s => s.User)
            .Include(em => em.ExamManementDetails)
            .Where(em => em.ExamPaper.LecturerId == lecturerId) 
            .AsQueryable();

        if (request.CourseId.HasValue)
            query = query.Where(em => em.ExamPaper.Lesson.CourseId == request.CourseId.Value);

        if (request.ExamId.HasValue)
            query = query.Where(em => em.ExamPaperId == request.ExamId.Value);

        var result = await query
            .OrderByDescending(em => em.ExamPaper.CreatedAt)
            .Select(em => new Response.SubmittedExamResponse
            {
                SubmissionId  = em.Id,
                StudentId     = em.StudentId,
                StudentName   = em.Student.User.FirstName + " " + em.Student.User.LastName,
                ExamId        = em.ExamPaperId,
                ExamTitle     = em.ExamPaper.Title,
                SubmittedAt   = em.ExamPaper.CreatedAt,
                // GRADED nếu tất cả câu tự luận đã có feedback, ngược lại UNGRADED
                GradingStatus = em.ExamManementDetails
                    .Where(d => !d.IsMultiChoice)
                    .All(d => d.Feedback != null)
                    ? "GRADED" : "UNGRADED",
            })
            .ToListAsync();

        return result;
    }
}