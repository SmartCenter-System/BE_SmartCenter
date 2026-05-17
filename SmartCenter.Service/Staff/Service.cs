using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity.Enums;
using SmartCenter.Service.Base;

namespace SmartCenter.Service.Staff;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }
    
    private Guid GetStaffrId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
                        .FindFirst("UserId")?.Value
                    ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng.");
    
        var roleClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(roleClaim) || !roleClaim.Equals("Staff", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Người dùng không phải là nhân viên.");
        }
        return Guid.Parse(claim);
    }

    public async Task<Response.ConsultationResponse> GetConsultations()
    {
        var pendingConsultations = await _dbContext.ConsultationRequests
            .Where(x => x.Status == ConsultReqStatus.Pending)
            .CountAsync();

        var today = DateTime.Today;
        var newStudentsToday = await _dbContext.Users
            .Where(x => x.CreatedAt.Date == today)
            .CountAsync();

        var pendingOrders = await _dbContext.Orders
            .Where(x => x.Status == OrderStatus.Pending)
            .CountAsync();

        return new Response.ConsultationResponse()
        {
            pendingConsultations = pendingConsultations,
            newStudentsToday = newStudentsToday,
            pendingOrders = pendingOrders
        };
    }

    public async Task<string> AcceptConsultation(Guid ConsultationId)
    {
        var staffrId = GetStaffrId();
        
        var Consultation = await _dbContext.ConsultationRequests
            .FirstOrDefaultAsync(x => x.Id == ConsultationId);

        if (Consultation == null)
        {
            throw new Exception("Đơn tư vẫn không tồn tại");
        }

        if (Consultation.StaffId != null)
        {
            throw new Exception("Đơn hàng đã được nhân viên khác xử lý");
        }

        Consultation.StaffId = staffrId;
        Consultation.Status = ConsultReqStatus.Accepted;
        Consultation.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return "Xác nhận yêu cầu thành công";
    }
    
    public async Task<string> RejectConsultation(Guid ConsultationId)
    {
        var staffrId = GetStaffrId();
        
        var Consultation = await _dbContext.ConsultationRequests
            .FirstOrDefaultAsync(x => x.Id == ConsultationId);

        if (Consultation == null)
        {
            throw new Exception("Đơn tư vẫn không tồn tại");
        }

        if (Consultation.StaffId != null)
        {
            throw new Exception("Đơn hàng đã được nhân viên khác xử lý");
        }

        Consultation.StaffId = staffrId;
        Consultation.Status = ConsultReqStatus.Rejected;
        Consultation.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return "Đã từ chối yêu cầu";
    }
    
    public async Task<string> ProcessingConsultation(Guid ConsultationId)
    {
        var staffrId = GetStaffrId();
        
        var Consultation = await _dbContext.ConsultationRequests
            .FirstOrDefaultAsync(x => x.Id == ConsultationId);

        if (Consultation == null)
        {
            throw new Exception("Đơn tư vẫn không tồn tại");
        }

        if (Consultation.StaffId != null)
        {
            throw new Exception("Đơn hàng đã được nhân viên khác xử lý");
        }

        Consultation.StaffId = staffrId;
        Consultation.Status = ConsultReqStatus.Processing;
        Consultation.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return "Đang tư vấn đơn yêu cầu";
    }

    public async Task<PagedResult<Response.ConsultationItemResponse>> GetConsultationsAsync(Request.ConsultationRequest request)
    {
        var query = _dbContext.ConsultationRequests.AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(c => c.Status == request.Status.Value);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.RequestDate)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new Response.ConsultationItemResponse()
            {
                Id = c.Id,
                FullName = c.FirstName + " " + c.LastName,
                Email = c.Email,
                Phone = c.PhoneNumber,
                CourseId = c.CourseId,
                CourseName = c.Course != null ? c.Course.CourseName : null,
                Note = c.Notes,
                Status = c.Status.ToString(),
                CreateAt = c.RequestDate,
            }).ToListAsync();

        return new PagedResult<Response.ConsultationItemResponse>()
        {
            Items = items,
            Total = total
        };
    }
    
    public async Task<PagedResult<Response.EnrollmentItemResponse>> GetEnrollmentsAsync(Request.GetEnrollmentsRequest request)
{
    var query = _dbContext.Enrollments
        .Include(e => e.Student)
            .ThenInclude(s => s.User)
        .Include(e => e.Course)
            .ThenInclude(c => c.Lessons)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(request.SearchName))
    {
        var keyword = request.SearchName.Trim().ToLower();
        query = query.Where(e =>
            (e.Student.User.FirstName + " " + e.Student.User.LastName)
            .ToLower().Contains(keyword));
    }

    if (request.CourseId.HasValue)
        query = query.Where(e => e.CourseId == request.CourseId.Value);

    var total = await query.CountAsync();

    var enrollments = await query
        .OrderByDescending(e => e.EnrollmentDate)
        .Skip((request.PageIndex - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToListAsync();

    // Tính progressPercent: số lesson đã hoàn thành / tổng lesson của course
    var items = new List<Response.EnrollmentItemResponse>();
    foreach (var e in enrollments)
    {
        var totalLessons = e.Course.Lessons.Count;
        var completedLessons = await _dbContext.LearningProcesses
            .CountAsync(lp => lp.StuId    == e.StuId
                           && lp.Lesson.CourseId == e.CourseId
                           && lp.IsCompleted);

        var progress = totalLessons > 0
            ? (int)Math.Round((double)completedLessons / totalLessons * 100)
            : 0;

        items.Add(new Response.EnrollmentItemResponse()
        {
            EnrollmentId    = e.Id,
            StudentId       = e.StuId,
            StudentName     = e.Student.User.FirstName + " " + e.Student.User.LastName,
            CourseId        = e.CourseId,
            CourseName      = e.Course.CourseName,
            ProgressPercent = progress,
            EnrolledAt      = e.EnrollmentDate,
        });
    }

    return new PagedResult<Response.EnrollmentItemResponse>
    {
        Items = items,
        Total = total,
    };
}
}