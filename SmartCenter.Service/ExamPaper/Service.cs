using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.ExamPaper;

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

    private async Task AuthorizeExamAsync(Guid examId)
    {
        var lecturerId = GetLecturerId();
        var owns = await _dbContext.ExamPapers
            .AnyAsync(e => e.Id == examId && e.LecturerId == lecturerId);
        if (!owns)
            throw new UnauthorizedAccessException("Bạn không có quyền thao tác với đề thi này.");
    }

    public async Task<List<Response.ExamResponse>> GetExamsByCourseAsync(Guid courseId)
    {
        var lecturerId = GetLecturerId();
        var query =  _dbContext.ExamPapers
            .Where(e => e.LecturerId == lecturerId && e.Lesson.CourseId == courseId)
            .Select(e => new Response.ExamResponse()
            {
                Id = e.Id,
                Title =  e.Title,
                TotalPoints =  e.TotalPoints,
                CountDown =  e.CountDown,
                LessonId =  e.LessonId,
                Status =   e.Status,
                CreateAt = e.CreatedAt,
                Deadline = new Response.DeadlineResponse()
                {
                    Id      = e.Deadline!.Id,
                    Title   = e.Deadline.Title,
                    EndedAt = e.Deadline.EndedAt,
                    Status  = e.Deadline.Status,
                }
            });
        var examPaper = await query.ToListAsync();
        return examPaper;
    }

    public async Task<Response.ExamResponse> CreateExamPaperAsync(Request.CreateExamPaperRequest request)
    {
        var lecturerId = GetLecturerId();

        var lesson = _dbContext.Lessons
            .FirstOrDefault(l => l.Id == request.LessonId && l.Section.Course.LecId == lecturerId);

        if (lesson == null)
            throw new Exception("Không tìm thấy bài học hoặc bạn không có quyền.");

        var exam = new Repository.Entity.ExamPaper()
        {
            Id = Guid.NewGuid(),
            LecturerId = lecturerId,
            LessonId = request.LessonId,
            Title = request.Title,
            CountDown = request.CountDown,
            TotalPoints = request.TotalPoints,
            Status = ExamPaperStatus.Open,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _dbContext.ExamPapers.Add(exam);
        
        var enrolledStudentIds = await _dbContext.Enrollments
            .Where(e => e.CourseId == lesson.CourseId && e.Status == EnrollmentStatus.Paid)
            .Select(e => e.Student.UserId)
            .ToListAsync();

        var notifications = enrolledStudentIds.Select(userId => new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Đề thi mới",
            Description = $"Đề thi \"{request.Title}\" vừa được tạo trong khóa học của bạn.",
            Type = "Email",
            RefId = exam.Id,
            RefType = "ExamPaper",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await _dbContext.Notifications.AddRangeAsync(notifications);
        await _dbContext.SaveChangesAsync();

        var query = _dbContext.ExamPapers
            .Where(e => e.Id == exam.Id)
            .Select(e => new Response.ExamResponse()
            {
                Id = e.Id,
                Title = e.Title,
                TotalPoints = e.TotalPoints,
                CountDown = e.CountDown,
                LessonId = e.LessonId,
                Status = e.Status,
                CreateAt = e.CreatedAt,
                Deadline = new Response.DeadlineResponse()
                {
                    Id = e.Deadline!.Id,
                    Title = e.Deadline.Title,
                    EndedAt = e.Deadline.EndedAt,
                    Status = e.Deadline.Status,
                }
            });
        var examPaper = await query.FirstAsync();
        return examPaper;

    }


    public async Task<Response.ExamResponse> UpdateExamPaperAsync(Guid examId, Request.UpdateExamPaperRequest request)
    {
        await AuthorizeExamAsync(examId);
        
        var exam = await _dbContext.ExamPapers.FindAsync(examId);
        if (exam == null)
            throw new Exception("Không tìm thấy đề thi.");
        
        if(request.Title != null) exam.Title = request.Title;
        if(request.CountDown != null) exam.CountDown = request.CountDown.Value;
        if(request.TotalPoints != null) exam.TotalPoints = request.TotalPoints.Value;
        if(request.Status != null) exam.Status = request.Status.Value;
        
        await _dbContext.SaveChangesAsync();
        
        var query = _dbContext.ExamPapers
            .Where(e => e.Id == exam.Id)
            .Select(e => new Response.ExamResponse()
            {
                Id = e.Id,
                Title = e.Title,
                TotalPoints = e.TotalPoints,
                CountDown = e.CountDown,
                LessonId = e.LessonId,
                Status = e.Status,
                CreateAt = e.CreatedAt,
                Deadline = new Response.DeadlineResponse()
                {
                    Id = e.Deadline!.Id,
                    Title = e.Deadline.Title,
                    EndedAt = e.Deadline.EndedAt,
                    Status = e.Deadline.Status,
                }
            });
        var examPaperUpdated = await query.FirstAsync();
        return examPaperUpdated;
    }

    public async Task<Response.DeadlineResponse> SetDeadlineAsync(Guid examId, Request.SetDeadlineRequest request)
    {
        await AuthorizeExamAsync(examId);

        var exam = await _dbContext.ExamPapers
            .Include(e => e.Deadline)
            .FirstOrDefaultAsync(e => e.Id == examId);
        if (exam == null)
            throw new Exception("Không tìm thấy đề thi");

        if (exam.Deadline != null)
        {
            exam.Deadline.Title = request.Title;
            exam.Deadline.Status = DeadlineStatus.Processing;
            exam.Deadline.EndedAt = request.EndedAt;
        }
        else
        {
            var deadline = new Deadline
            {
                Id          = Guid.NewGuid(),
                ExamPaperId = examId,
                Title       = request.Title,
                EndedAt     = request.EndedAt,
                Status      = DeadlineStatus.Processing,
            };
            _dbContext.Deadlines.Add(deadline);
        }

        await _dbContext.SaveChangesAsync();

        var deadlineResponse = new Response.DeadlineResponse()
        {
            Id = exam.Deadline!.Id,
            Title = exam.Deadline.Title,
            EndedAt = exam.Deadline.EndedAt,
            Status = exam.Deadline.Status,
        };

        return deadlineResponse;
    }
    
    public async Task DeleteExamPaperAsync(Guid examId)
    {
        await AuthorizeExamAsync(examId);

        var exam = await _dbContext.ExamPapers.FindAsync(examId)
                   ?? throw new Exception("Không tìm thấy đề thi.");

        var hasSubmission = await _dbContext.ExamManagements
            .AnyAsync(e => e.ExamPaperId == examId);

        if (hasSubmission)
            throw new Exception("Không thể xóa đề thi đã có học sinh làm bài.");

        _dbContext.ExamPapers.Remove(exam);
        await _dbContext.SaveChangesAsync();
    }
}