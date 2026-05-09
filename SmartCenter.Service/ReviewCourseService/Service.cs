using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.ReviewCourseService;

/// <inheritdoc />
public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetStudentId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
                        .FindFirst("studentId")?.Value
                    ?? throw new UnauthorizedAccessException("Không phải học sinh thì đánh giá ăn bìu à");
        return Guid.Parse(claim);
    }


    public async Task<Response.ReviewResponse> CreateReviewCourseAsync(Request.CreateReviewRequest request)
    {
        var studentId = GetStudentId();

        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new ArgumentException("Điểm đánh giá phải nằm trong khoảng từ 1 đến 5.");
        }

        var courseExists = await _dbContext.Courses
            .AnyAsync(c => c.Id == request.CourseId && c.IsDeleted == false);

        if (!courseExists)
        {
            throw new Exception("Khóa học không tồn tại hoặc đã bị gỡ bỏ.");
        }

        var hasEnrolled = await _dbContext.Enrollments
            .AnyAsync(e => e.StuId == studentId
                           && e.CourseId == request.CourseId
                           && e.Status == EnrollmentStatus.Paid);
        if (!hasEnrolled)
        {
            throw new UnauthorizedAccessException("Bạn chưa đăng ký hoặc chưa hoàn tất thanh toán cho khóa học này.");
        }

        var newReview = new ReviewCourse()
        {
            Id = Guid.NewGuid(),
            StuId = studentId,
            CourseId = request.CourseId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        _dbContext.ReviewCourses.Add(newReview);
        await _dbContext.SaveChangesAsync();

        return new Response.ReviewResponse()
        {
            ReviewId = newReview.Id,
            CourseId = newReview.CourseId,
            Rating = newReview.Rating,
            Comment = newReview.Comment,
            CreatedAt = newReview.CreatedAt
        };
    }

    public async Task<List<Response.ReviewDetailResponse>> GetReviewCourseAsync(Guid courseId, Guid? studentId)
    {
        var courseExists = await _dbContext.Courses
            .AnyAsync(c => c.Id == courseId && c.IsDeleted == false);

        if (!courseExists)
        {
            throw new Exception("Khóa học không tồn tại hoặc đã bị gỡ bỏ.");
        }

        var query = _dbContext.ReviewCourses
            .Include(r => r.Student)
            .ThenInclude(s => s.User)
            .Where(r => r.CourseId == courseId && r.IsDeleted == false)
            .AsQueryable();

        if (studentId.HasValue)
        {
            query = query.Where(r => r.StuId == studentId.Value);
        }

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new Response.ReviewDetailResponse()
            {
                ReviewId = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                StudentId = r.StuId,
                StudentName = (r.Student.User.FirstName + " " + r.Student.User.LastName).Trim()
            })
            .ToListAsync();
        return reviews;
    }
}