using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity.Enums;

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
}