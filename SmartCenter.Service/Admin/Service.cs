using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity.Enums;
using SmartCenter.Service.Base;

namespace SmartCenter.Service.Admin;

public class Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<Response.UserItemResponse>> GetUsersAsync(Request.GetUsersRequest request)
    {
        var query = _dbContext.Users.AsQueryable();

        if (request.Role.HasValue)
            query = query.Where(u => u.Role == request.Role.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(u => (u.FirstName + " " + u.LastName).ToLower().Contains(keyword)
                                     || u.Email.ToLower().Contains(keyword));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new Response.UserItemResponse
            {
                Id = u.Id,
                FullName = u.FirstName + " " + u.LastName,
                Email = u.Email,
                Role = u.Role.ToString(),
                Status = u.Status.ToString(),
                CreatedAt = u.CreatedAt,
            })
            .ToListAsync();

        return new PagedResult<Response.UserItemResponse>
        {
            Items = items,
            Total = total,
        };
    }

    public async Task<PagedResult<Response.OrderItemResponse>> GetOrdersAsync(Request.GetOrdersRequest request)
    {
        var query = _dbContext.Orders
            .Include(o => o.Student)
            .ThenInclude(s => s.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Course)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(o => o.Status == request.Status.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new Response.OrderItemResponse
            {
                OrderId = o.Id,
                OrderCode = o.OrderCode,
                StudentName = o.Student.User.FirstName + " " + o.Student.User.LastName,
                StudentEmail = o.Student.User.Email,
                CourseNames = o.OrderItems
                    .Where(oi => oi.Course != null)
                    .Select(oi => oi.Course!.CourseName)
                    .ToList(),
                TotalAmount = o.TotalAmount,
                PaymentStatus = o.Status.ToString(),
                CreatedAt = o.CreatedAt,
            })
            .ToListAsync();

        return new PagedResult<Response.OrderItemResponse>
        {
            Items = items,
            Total = total,
        };
    }

    public async Task LockUserAsync(Guid userId)
    {
        var user = await _dbContext.Users.FindAsync(userId)
                   ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        if (user.Status == UserStatus.Inactive)
            throw new InvalidOperationException("Tài khoản này đã bị khóa rồi.");

        user.Status = UserStatus.Inactive;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<Response.DashBoardCourseResponse> GetDashBoardCourseAsync(int? year)
    {
        var Query = _dbContext.Courses.Where(x => true);
        if (year != null)
        {
            Query = _dbContext.Courses.Where(x => x.AcademicYear == year);
        }

        var ListCourseIds = await Query.Select(x => x.Id).ToListAsync();

        if (!ListCourseIds.Any())
        {
            throw new Exception("Không có khóa học nào tồn tại");
        }

        var TotalCourse = ListCourseIds.Count;

        var Enrollment = _dbContext.Enrollments
            .Include(x => x.Transaction)
            .Where(x => ListCourseIds.Contains(x.CourseId));

        var TotalStudent = Enrollment.Select(x => x.StuId).Count();

        var totalRevenue = await Enrollment.SumAsync(x => x.Transaction.Amount);
        

        var revenueByMonthRaw = await Enrollment
            .GroupBy(x => new { x.EnrollmentDate.Year, x.EnrollmentDate.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalAmount = g.Sum(x => x.Transaction.Amount)
            })
            .ToListAsync();
        
        var revenueChart = revenueByMonthRaw
            .Select(x => new Response.revenueChart
            {
                month = $"{x.Month}/{x.Year}",
                revenue = x.TotalAmount
            })
            .OrderBy(x => x.month)
            .ToList();

        return new Response.DashBoardCourseResponse
        {
            totalCourses = TotalCourse,
            totalStudents = TotalStudent,
            totalRevenue = totalRevenue,
            revenueCharts = revenueChart
        };
    }

    public async Task UnLockUserAsync(Guid userId)
    {
        var user = await _dbContext.Users.FindAsync(userId)
                   ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        if (user.Status == UserStatus.Active)
            throw new InvalidOperationException("Tài khoản này đã mở rồi.");

        user.Status = UserStatus.Active;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<Response.UserDetailResponse> GetUserDetailAsync(Guid userId)
    {
        var userExist = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (userExist == null)
            throw new Exception("Nguời dùng không tìm thấy.");

        var user = new Response.UserDetailResponse()
        {
            Id = userId,
            FullName = userExist.LastName + " " + userExist.FirstName,
            Email = userExist.Email,
            Role = userExist.Role,
            Status = userExist.Status,
            CreatedAt = userExist.CreatedAt,
        };

        return user;
    }
}