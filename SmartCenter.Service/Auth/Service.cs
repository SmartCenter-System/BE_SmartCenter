using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity;
using SmartCenter.Repository.Entity.Enums;
using SmartCenter.Service.JwtService;
using SmartCenter.Service.MailService;
using SmartCenter.Repository.Entity;

namespace SmartCenter.Service.Auth;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    private readonly IJwtService _jwtService;
    private readonly MailService.IService _mailService;

    public Service(AppDbContext dbContext, IJwtService jwtService, MailService.IService mailService)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _mailService = mailService;
    }
    
    public async Task<string> Register(Request.RegisterRequest request)
{
    var transaction = await _dbContext.Database.BeginTransactionAsync();
    try
    {
        var emailExists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
            throw new ArgumentException($"Email {request.Email} đã tồn tại");

        var passwordHash = HashPassword(request.Password);
        var verifiedCode = new Random().Next(100000, 999999);

        var user = new User()
        {
            Id           = Guid.NewGuid(),
            FirstName    = request.FirstName,
            LastName     = request.LastName,
            Email        = request.Email,
            PasswordHash = passwordHash,
            Phone        = request.Phone ?? "",
            Role         = UserRole.Student,
            Status       = UserStatus.Active,
            Verified     = false,
            VerifiedCode = verifiedCode,
            CreatedAt    = DateTimeOffset.UtcNow
        };
        _dbContext.Users.Add(user);

        var student = new Repository.Entity.Student
        {
            Id             = Guid.NewGuid(),
            UserId         = user.Id,
            Address        = "",
            City           = "",
            EnrollmentDate = DateTimeOffset.UtcNow,
            CreatedAt      = DateTimeOffset.UtcNow
        };
        _dbContext.Students.Add(student);

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        _ = Task.Run(async () =>
        {
            try
            {
                await _mailService.SendMail(new MailContent
                {
                    To      = request.Email,
                    Subject = "SmartCenter – Mã xác thực email của bạn",
                    Body    = BuildVerificationEmailBody($"{request.FirstName} {request.LastName}", verifiedCode)
                });
            }
            catch { /* log lỗi nếu cần, không throw */ }
        });

        return "Đăng ký thành công! Vui lòng kiểm tra email để lấy mã xác thực.";
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}

    public async Task<Response.AuthResponse> VerifyEmail(int code)
    {
        var user = await _dbContext.Users
                       .Include(u => u.Student).Include(user => user.Lecturer)
                       .FirstOrDefaultAsync(u => u.VerifiedCode == code && !u.Verified)
                   ?? throw new KeyNotFoundException("Mã xác thực không hợp lệ hoặc đã được sử dụng.");

        user.Verified    = true;
        user.VerifiedCode = 0;
        user.UpdatedAt   = DateTimeOffset.UtcNow;

        if (user.Student != null && user.Student.CartId == Guid.Empty)
        {
            var cart = new Repository.Entity.Cart()
            {
                Id    = Guid.NewGuid(),
                StuId = user.Student.Id,
            };
            _dbContext.Carts.Add(cart);
            user.Student.CartId = cart.Id;
        }

        await _dbContext.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new Claim("UserId",        user.Id.ToString()),
            new Claim("Email",         user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        };

        if (user.Student != null)
            claims.Add(new Claim("studentId", user.Student.Id.ToString()));
        if (user.Lecturer != null) claims.Add(new("lecturerId", user.Lecturer.Id.ToString()));

        var accessToken = _jwtService.GenerateAccessToken(claims);

        return new Response.AuthResponse
        {
            UserId      = user.Id,
            Email       = user.Email,
            Fullname    = $"{user.FirstName} {user.LastName}",
            Role        = user.Role.ToString(),
            AccessToken = accessToken,
        };
    }

    public async Task<Response.AuthResponse> Login(Request.LoginRequest request)
    {
        var user = await _dbContext.Users
            .Include(u => u.Student)
            .Include(u => u.Lecturer) 
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            throw new KeyNotFoundException("Email hoặc mật khẩu không đúng.");

        if (!user.Verified)
            throw new InvalidOperationException("Tài khoản chưa được xác thực email.");

        if (user.PasswordHash != HashPassword(request.Password))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        var claims   = BuildClaims(user);
        
        var accessToken = _jwtService.GenerateAccessToken(claims);
        var refreshToken = _jwtService.GenerateRefreshToken();
        
        var session = new UserSession
        {
            Id                = Guid.NewGuid(),
            UserId            = user.Id,
            RefreshToken      = refreshToken,
            DeviceFingerprint = request.DeviceFingerprint ?? "unknown",
            ExpiresAt         = DateTimeOffset.UtcNow.AddDays(7),
            IsRevoked         = false,
            CreatedAt         = DateTimeOffset.UtcNow,
        };
        _dbContext.UserSessions.Add(session);
        await _dbContext.SaveChangesAsync();
        
        return new Response.AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Fullname = $"{user.FirstName} {user.LastName}",
            Role = user.Role.ToString(),
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<string> ForgotPassword(Request.ForgotPasswordRequest request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        
        if(user == null)
            return "Vui lòng kiểm tra email để nhận hướng dẫn đặt lại mật khẩu.";

        if (!user.Verified)
            throw new InvalidOperationException("Tài khoản chưa được xác thực email");
        
        var resetCode = new Random().Next(100000, 999999);
        user.ResetPasswordCode = resetCode;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        
        await _dbContext.SaveChangesAsync();

        await _mailService.SendMail(new MailContent
        {
            To = request.Email,
            Subject = "SmartCenter - Mã đặt lại mật khẩu",
            Body = BuildVerificationEmailBody($"{user.FirstName} {user.LastName}", resetCode)
        });

        return "Vui lòng kiểm tra email để nhận hướng dẫn đặt lại mật khẩu.";
    }

    public async Task<string> ResetPassword(Request.ResetPasswordRequest request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.ResetPasswordCode == request.Code);

        if (user == null)
            throw new KeyNotFoundException("Mã đặt lại mật khẩu không hợp lệ hoặc đã được sử dụng");

        user.PasswordHash = HashPassword(request.NewPassword);
        user.ResetPasswordCode = 0;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        
        await _dbContext.SaveChangesAsync();
        return "Đặt lại mật khẩu thành công! Vui lòng đăng nhập lại.";
    }

    public async Task<Response.LecturerRegisterResponse> RegisterLecturer(Request.RegisterLecturerRequest request)
    {
        var emailExists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
            throw new ArgumentException("Email đã được sử dụng.");
 
        // Admin tạo tk -> Verified = true lun, kh cần xác thực email
        var user = new User
        {
            Id           = Guid.NewGuid(),
            FirstName    = request.FirstName,
            LastName     = request.LastName,
            Email        = request.Email,
            PasswordHash = HashPassword(request.Password),
            Phone        = request.Phone ?? string.Empty,
            Role         = UserRole.Lecturer,
            Status       = UserStatus.Active,
            Verified     = true,
            VerifiedCode = 0,
            CreatedAt    = DateTimeOffset.UtcNow,
        };
        _dbContext.Users.Add(user);
 
        var lecturer = new Lecturer
        {
            Id        = Guid.NewGuid(),
            UserId    = user.Id,
            Bio       = request.Bio,
            Expertise = request.Expertise,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _dbContext.Lecturers.Add(lecturer);
        await _dbContext.SaveChangesAsync();
 
        // Gửi email thông báo thông tin đăng nhập cho giáo viên
        // Sau SaveChangesAsync(), gửi mail bất đồng bộ
        _ = Task.Run(async () =>
        {
            try
            {
                await _mailService.SendMail(new MailContent
                {
                    To      = request.Email,
                    Subject = "SmartCenter – Tài khoản giảng viên của bạn đã được tạo",
                    Body    = BuildLecturerWelcomeEmailBody(
                        $"{request.FirstName} {request.LastName}",
                        request.Email,
                        request.Password)
                });
            }
            catch { /**/ }
        });
 
        return new Response.LecturerRegisterResponse
        {
            UserId     = user.Id,
            LecturerId = lecturer.Id,
            FullName   = $"{user.FirstName} {user.LastName}",
            Email      = user.Email,
            Expertise  = lecturer.Expertise,
        };
    }

    public async Task<Response.AuthResponse> RefreshToken(string refreshToken)
    {
        var session = await _dbContext.UserSessions
              .Include(s => s.User)
                .ThenInclude(u => u!.Student)
              .Include(s => s.User)
                .ThenInclude(u => u!.Lecturer)
              .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken && !s.IsRevoked)
          ?? throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn.");

        if (session.ExpiresAt < DateTimeOffset.UtcNow)
        {
            session.IsRevoked = true;
            await _dbContext.SaveChangesAsync();
            throw new UnauthorizedAccessException("Refresh token đã hết hạn. Vui lòng đăng nhập lại.");
        }

        var user = session.User;

        session.IsRevoked = true;

        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var newSession = new UserSession
        {
            Id                = Guid.NewGuid(),
            UserId            = user!.Id,
            RefreshToken      = newRefreshToken,
            DeviceFingerprint = session.DeviceFingerprint,
            ExpiresAt         = DateTimeOffset.UtcNow.AddDays(7),
            IsRevoked         = false,
            CreatedAt         = DateTimeOffset.UtcNow,
        };
        _dbContext.UserSessions.Add(newSession);
        await _dbContext.SaveChangesAsync();

        var claims      = BuildClaims(user);
        var accessToken = _jwtService.GenerateAccessToken(claims);

        return new Response.AuthResponse
        {
            UserId       = user.Id,
            Email        = user.Email,
            Fullname     = $"{user.FirstName} {user.LastName}",
            Role         = user.Role.ToString(),
            AccessToken  = accessToken,
            RefreshToken = newRefreshToken,
        };
    }

    public async Task Logout(string refreshToken)
    {
        var session = await _dbContext.UserSessions
            .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken && !s.IsRevoked);

        if (session == null) return; 

        session.IsRevoked = true;
        await _dbContext.SaveChangesAsync();
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
    
    private static List<Claim> BuildClaims(User user)
    {
        var claims = new List<Claim>
        {
            new("UserId",        user.Id.ToString()),
            new("Email",         user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
        };
        if (user.Student  != null) claims.Add(new("studentId",  user.Student.Id.ToString()));
        if (user.Lecturer != null) claims.Add(new("lecturerId", user.Lecturer.Id.ToString()));
        return claims;
    }
    
    
    private static string BuildVerificationEmailBody(string fullName, int verifiedCode) => $"""
        <!DOCTYPE html>
        <html lang="vi">
        <head>
            <meta charset="UTF-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
            <title>SmartCenter Verify</title>
        </head>
        
        <body style="
            margin:0;
            padding:0;
            background:#eef4ff;
            font-family:Arial,Helvetica,sans-serif;
        ">
        
        <table width="100%" cellpadding="0" cellspacing="0" border="0">
        <tr>
        <td align="center" style="padding:40px 16px;">
        
            <!-- Container -->
            <table width="600" cellpadding="0" cellspacing="0" border="0"
                   style="
                        background:#ffffff;
                        border-radius:28px;
                        overflow:hidden;
                        box-shadow:0 12px 40px rgba(37,99,235,0.15);
                   ">
        
                <!-- HEADER -->
                <tr>
                    <td style="
                        background:linear-gradient(135deg,#2563EB 0%, #1D4ED8 60%, #1E40AF 100%);
                        padding:30px 32px;
                        text-align:center;
                        position:relative;
                    ">
        
                        <!-- Floating Circle -->
                        <div style=" width:300px; height:72px; margin:0 auto 0px; border-radius:20px; background:rgba(221, 8, 8, 0.15); line-height:72px; text-align:center; font-size:28px; font-weight:800; color:rgb(255, 255, 255); border:1px solid rgba(255,255,255,0.2); "> SmartCenter </div>
        
                        <!-- <h1 style="
                            margin:0;
                            color:#ffffff;
                            font-size:36px;
                            font-weight:800;
                            letter-spacing:1px;
                        ">
                            SmartCenter
                        </h1> -->
        
                        <p style="
                            margin:16px 0 0;
                            color:rgba(255,255,255,0.9);
                            font-size:16px;
                            line-height:1.7;
                        ">
                            Học thông minh • Bứt phá tương lai 
                        </p>
        
                    </td>
                </tr>
        
                <!-- BODY -->
                <tr>
                    <td style="padding:50px 40px; color:#1E293B;">
        
                        <p style="
                            margin:0 0 18px;
                            font-size:26px;
                            font-weight:700;
                            color:#0F172A;
                        ">
                            Xin chào <strong style="color:rgb(255, 208, 0);">{fullName}</strong>,
                        </p>
        
                        <p style="
                            margin:0 0 18px;
                            font-size:16px;
                            line-height:1.9;
                            color:#475569;
                        ">
                            Cảm ơn bạn đã đăng ký tài khoản tại
                            <strong style="color:#2563EB;">SmartCenter</strong>.
                        </p>
        
                        <p style="
                            margin:0 0 36px;
                            font-size:16px;
                            line-height:1.9;
                            color:#475569;
                        ">
                            Sử dụng mã xác thực bên dưới để kích hoạt tài khoản của bạn.
                        </p>
        
                        <!-- OTP CARD -->
                        <div style="
                            background:linear-gradient(135deg,#2563EB 0%, #1D4ED8 100%);
                            border-radius:24px;
                            padding:40px 20px;
                            text-align:center;
                            margin:40px 0;
                            box-shadow:0 12px 30px rgba(37,99,235,0.25);
                            position:relative;
                            overflow:hidden;
                        ">
        
                            <!-- Glow -->
                            <div style="
                                position:absolute;
                                width:200px;
                                height:200px;
                                background:rgba(255,255,255,0.08);
                                border-radius:50%;
                                top:-80px;
                                right:-60px;
                            "></div>
        
                            <p style="
                                margin:0 0 16px;
                                color:#DBEAFE;
                                font-size:14px;
                                letter-spacing:2px;
                                text-transform:uppercase;
                            ">
                                Verification Code
                            </p>
        
                            <div style="
                                display:inline-block;
                                background:#ffffff;
                                padding:18px 34px;
                                border-radius:18px;
                                box-shadow:0 8px 24px rgba(0,0,0,0.15);
                            ">
                                <span style="
                                    font-size:32px;
                                    font-weight:800;
                                    letter-spacing:12px;
                                    color:#FACC15;
                                ">
                                    {verifiedCode}
                                </span>
                            </div>
        
                            <p style="
                                margin:18px 0 0;
                                color:#DBEAFE;
                                font-size:13px;
                            ">
                                Mã xác thực có hiệu lực trong 5 phút
                            </p>
        
                        </div>
        
                        <!-- BUTTON -->
                        <div style="text-align:center; margin:42px 0;">
        
                            <a href="#"
                               style="
                                    display:inline-block;
                                    background:#FACC15;
                                    color:#1E3A8A;
                                    text-decoration:none;
                                    padding:16px 34px;
                                    border-radius:16px;
                                    font-size:15px;
                                    font-weight:700;
                                    box-shadow:0 10px 24px rgba(250,204,21,0.35);
                               ">
                                Xác thực tài khoản
                            </a>
        
                        </div>
        
                        <!-- INFO BOX -->
                        <div style="
                            background:#F8FAFC;
                            border:1px solid #E2E8F0;
                            border-left:5px solid #FACC15;
                            border-radius:16px;
                            padding:18px 20px;
                        ">
        
                            <p style="
                                margin:0;
                                color:#64748B;
                                font-size:14px;
                                line-height:1.8;
                            ">
                                Nếu bạn không yêu cầu tạo tài khoản,
                                hãy bỏ qua email này để đảm bảo an toàn.
                            </p>
        
                        </div>
        
                    </td>
                </tr>
        
                <!-- FOOTER -->
                <tr>
                    <td style="
                        background:#0F172A;
                        padding:34px 24px;
                        text-align:center;
                    ">
        
                        <h3 style="
                            margin:0 0 10px;
                            color:#ffffff;
                            font-size:20px;
                        ">
                            SmartCenter
                        </h3>
        
                        <p style="
                            margin:0 0 16px;
                            color:#94A3B8;
                            font-size:14px;
                            line-height:1.7;
                        ">
                            Nền tảng học tập hiện đại dành cho học sinh Việt Nam.
                        </p>
        
                        <p style="
                            margin:0;
                            color:#64748B;
                            font-size:12px;
                        ">
                            © 2026 SmartCenter. All rights reserved.
                        </p>
        
                    </td>
                </tr>
        
            </table>
        
        </td>
        </tr>
        </table>
        
        </body>
        </html>
        
        
        """;
    private static string BuildLecturerWelcomeEmailBody(string fullName, string email, string password) => $"""
        <!DOCTYPE html>
        <html lang="vi">
        <head>
            <meta charset="UTF-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
            <title>SmartCenter Lecturer Account</title>
        </head>
        
        <body style="
            margin:0;
            padding:0;
            background:#eef4ff;
            font-family:Arial,Helvetica,sans-serif;
        ">
        
        <table width="100%" cellpadding="0" cellspacing="0" border="0">
        <tr>
        <td align="center" style="padding:40px 16px;">
        
            <!-- Container -->
            <table width="600" cellpadding="0" cellspacing="0" border="0"
                   style="
                        background:#ffffff;
                        border-radius:24px;
                        overflow:hidden;
                        box-shadow:0 12px 40px rgba(37,99,235,0.12);
                   ">
        
                <!-- Header -->
                <tr>
                    <td style="
                        background:linear-gradient(135deg,#2563EB 0%, #1D4ED8 100%);
                        padding:48px 32px;
                        text-align:center;
                    ">
        
                        <!-- Logo -->
                        <!-- <img src="YOUR_LOGO_URL"
                             alt="SmartCenter"
                             width="72"
                             style="
                                display:block;
                                margin:0 auto 24px;
                             " /> -->
        
                        <h1 style="
                            margin:0;
                            color:#ffffff;
                            font-size:36px;
                            font-weight:800;
                            letter-spacing:1px;
                        ">
                            SmartCenter
                        </h1>
        
                        <p style="
                            margin:16px 0 0;
                            color:rgba(255,255,255,0.9);
                            font-size:16px;
                            line-height:1.7;
                        ">
                            Hệ thống quản lý học tập thông minh
                        </p>
        
                    </td>
                </tr>
        
                <!-- Body -->
                <tr>
                    <td style="padding:50px 40px; color:#1E293B;">
        
                        <p style="
                            margin:0 0 18px;
                            font-size:26px;
                            font-weight:700;
                            color:#0F172A;
                        ">
                            Xin chào <strong style="color:#FACC15;">{fullName}</strong>,
                        </p>
        
                        <p style="
                            margin:0 0 18px;
                            font-size:16px;
                            line-height:1.9;
                            color:#475569;
                        ">
                            Tài khoản giảng viên của bạn trên
                            <strong style="color:#2563EB;">SmartCenter</strong>
                            đã được tạo thành công.
                        </p>
        
                        <p style="
                            margin:0 0 34px;
                            font-size:16px;
                            line-height:1.9;
                            color:#475569;
                        ">
                            Bạn có thể sử dụng thông tin bên dưới để đăng nhập vào hệ thống.
                        </p>
        
                        <!-- Login Info Card -->
                        <table width="100%" cellpadding="0" cellspacing="0"
                               style="
                                    background:#F8FAFC;
                                    border:1px solid #E2E8F0;
                                    border-radius:20px;
                                    padding:30px;
                                    margin:32px 0;
                               ">
        
                            <tr>
                                <td style="
                                    padding-bottom:24px;
                                    border-bottom:1px solid #E2E8F0;
                                ">
        
                                    <p style="
                                        margin:0 0 8px;
                                        font-size:13px;
                                        color:#64748B;
                                        text-transform:uppercase;
                                        letter-spacing:1px;
                                    ">
                                        Email đăng nhập
                                    </p>
        
                                    <p style="
                                        margin:0;
                                        font-size:18px;
                                        color:#2563EB;
                                        font-weight:700;
                                        word-break:break-all;
                                    ">
                                        {email}
                                    </p>
        
                                </td>
                            </tr>
        
                            <tr>
                                <td style="padding-top:24px;">
        
                                    <p style="
                                        margin:0 0 8px;
                                        font-size:13px;
                                        color:#64748B;
                                        text-transform:uppercase;
                                        letter-spacing:1px;
                                    ">
                                        Mật khẩu tạm thời
                                    </p>
        
                                    <p style="
                                        margin:0;
                                        font-size:18px;
                                        color:#F59E0B;
                                        font-weight:700;
                                        letter-spacing:1px;
                                    ">
                                        {password}
                                    </p>
        
                                </td>
                            </tr>
        
                        </table>
        
                        <!-- Warning -->
                        <div style="
                            background:#FEF3C7;
                            border-left:5px solid #FACC15;
                            border-radius:14px;
                            padding:18px 20px;
                            margin-top:36px;
                        ">
        
                            <p style="
                                margin:0;
                                color:#92400E;
                                font-size:14px;
                                line-height:1.8;
                            ">
                                Vui lòng thay đổi mật khẩu ngay sau lần đăng nhập đầu tiên
                                để đảm bảo an toàn cho tài khoản của bạn.
                            </p>
        
                        </div>
        
                        <!-- Button -->
                        <div style="text-align:center; margin-top:42px;">
        
                            <a href="#"
                               style="
                                    display:inline-block;
                                    background:#2563EB;
                                    color:#ffffff;
                                    text-decoration:none;
                                    padding:16px 34px;
                                    border-radius:14px;
                                    font-size:15px;
                                    font-weight:700;
                                    box-shadow:0 10px 24px rgba(37,99,235,0.25);
                               ">
                                Đăng nhập hệ thống
                            </a>
        
                        </div>
        
                    </td>
                </tr>
        
                <!-- Footer -->
                <tr>
                    <td style="
                        background:#0F172A;
                        padding:34px 24px;
                        text-align:center;
                    ">
        
                        <p style="
                            margin:0 0 10px;
                            color:#ffffff;
                            font-size:20px;
                            font-weight:700;
                        ">
                            SmartCenter
                        </p>
        
                        <p style="
                            margin:0 0 16px;
                            color:#94A3B8;
                            font-size:14px;
                            line-height:1.7;
                        ">
                            Nền tảng học tập hiện đại dành cho giáo viên và học sinh.
                        </p>
        
                        <p style="
                            margin:0;
                            color:#64748B;
                            font-size:12px;
                        ">
                            © 2026 SmartCenter. All rights reserved.
                        </p>
        
                    </td>
                </tr>
        
            </table>
        
        </td>
        </tr>
        </table>
        
        </body>
        </html>
        
        """;
}