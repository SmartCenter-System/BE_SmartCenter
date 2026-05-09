using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.UserService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
                        .FindFirst("UserId")?.Value
                    ?? throw new UnauthorizedAccessException("Hình như chưa đăng nhập bạn ơi :)))");
        return Guid.Parse(claim);
    }


    public async Task<Response.UserProfileResponse> GetProfileAsync()
    {
        var userId = GetUserId();

        var user = await _dbContext.Users
            .Include(u => u.Student) 
            .Include(u => u.Lecturer)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new Exception("Không tìm thấy thông tin người dùng trong hệ thống.");
        }

        var response = new Response.UserProfileResponse
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            Phone = user.Phone,
            ImgUrl = user.ImgUrl
        };
        
        if (user.Role == UserRole.Student && user.Student != null)
        {
            response.Address = user.Student.Address;
            response.City = user.Student.City;
            response.ZaloLink = user.Student.ZaloLink;
        }
        else if (user.Role == UserRole.Lecturer && user.Lecturer != null)
        {
            response.Bio = user.Lecturer.Bio;
            response.Expertise = user.Lecturer.Expertise;
        }

        return response;
    }

    public async Task<Response.UserProfileResponse> UpdateProfileAsync(Request.UpdateProfileRequest request)
    {
        var userId = GetUserId();

        var user = await _dbContext.Users
            .Include(u => u.Student)
            .Include(u => u.Lecturer)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new Exception("Không tìm thấy tài khoản để cập nhật.");
        }
        
        if (request.FirstName != null)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName))
                throw new Exception("Tên (FirstName) không được truyền chuỗi rỗng.");
            user.FirstName = request.FirstName.Trim();
        }

        if (request.LastName != null)
        {
            if (string.IsNullOrWhiteSpace(request.LastName))
                throw new Exception("Họ (LastName) không được truyền chuỗi rỗng.");
            user.LastName = request.LastName.Trim();
        }

        if (request.Phone != null)
        {
            if (string.IsNullOrWhiteSpace(request.Phone))
                throw new Exception("Số điện thoại không được truyền chuỗi rỗng.");
            user.Phone = request.Phone.Trim();
        }

        if (request.ImgUrl != null)
        {
            user.ImgUrl = request.ImgUrl.Trim();
        }

        if (request.Email != null && request.Email != user.Email)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new Exception("Email không được truyền chuỗi rỗng.");

            var emailExists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email);
            if (emailExists) throw new Exception("Email này đã được sử dụng bởi một tài khoản khác.");
            user.Email = request.Email.Trim();
        }
        
        if (user.Role == UserRole.Student) 
        {
            if (user.Student == null)
            {
                throw new Exception("Hồ sơ học sinh của bạn không tồn tại. Không thể cập nhật.");
            }
            if (request.Address != null)
            {
                if (string.IsNullOrWhiteSpace(request.Address)) throw new Exception("Địa chỉ không được truyền chuỗi rỗng.");
                user.Student.Address = request.Address.Trim();
            }

            if (request.City != null)
            {
                if (string.IsNullOrWhiteSpace(request.City)) throw new Exception("Thành phố không được truyền chuỗi rỗng.");
                user.Student.City = request.City.Trim();
            }

            if (request.ZaloLink != null)
            {
                if (string.IsNullOrWhiteSpace(request.ZaloLink)) throw new Exception("Link Zalo không được truyền chuỗi rỗng.");
                user.Student.ZaloLink = request.ZaloLink.Trim();
            }
        }
        else if (user.Role == UserRole.Lecturer) 
        {
            if (user.Lecturer == null)
            {
                throw new Exception("Hồ sơ giảng viên của bạn không tồn tại. Không thể cập nhật.");
            }
            
            if (request.Bio != null)
            {
                if (string.IsNullOrWhiteSpace(request.Bio)) throw new Exception("Bio không được truyền chuỗi rỗng.");
                user.Lecturer.Bio = request.Bio.Trim();
            }

            if (request.Expertise != null)
            {
                if (string.IsNullOrWhiteSpace(request.Expertise)) throw new Exception("Chuyên môn không được truyền chuỗi rỗng.");
                user.Lecturer.Expertise = request.Expertise.Trim();
            }
        }
        await _dbContext.SaveChangesAsync();

        return await GetProfileAsync();
    }
}