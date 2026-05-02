using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity;
using SmartCenter.Repository.Entity.Enums;
using SmartCenter.Service.JwtService;
using SmartCenter.Service.MailService;

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
        {
            throw new ArgumentException($"Email {request.Email} đã tồn tại");
        }
        
        var passwordHash = HashPassword(request.Password);
        
        var verifiedCode = new Random().Next(100000, 999999);

        var user = new User()
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = passwordHash,
            Phone = request.Phone ?? "",
            Role = UserRole.Student,
            Status = UserStatus.Active,
            Verified = false,
            VerifiedCode = verifiedCode,
            CreatedAt = DateTimeOffset.UtcNow

        };
        _dbContext.Users.Add(user);

        var student = new Repository.Entity.Student
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Address = "", 
            City = "",    
            EnrollmentDate = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _dbContext.Students.Add(student);
        await _dbContext.SaveChangesAsync();
        
        await _mailService.SendMail(new MailContent
        {
            To = request.Email,
            Subject = "SmartCenter – Mã xác thực email của bạn",
            Body = BuildVerificationEmailBody($"{request.FirstName} {request.LastName}", verifiedCode)
        });
        await transaction.CommitAsync();
        return "Đăng ký thành công! Vui lòng kiểm tra email để lấy mã xác thực.";
        }
        catch (Exception e)
        {
            await  transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Response.AuthResponse> VerifyEmail(int code)
    {
        var user = await _dbContext.Users
            .Include(u => u.Student)
            .FirstOrDefaultAsync(u => u.VerifiedCode == code && !u.Verified);
 
        if (user == null)
            throw new KeyNotFoundException("Mã xác thực không hợp lệ hoặc đã được sử dụng.");
        
        user.Verified = true;
        user.VerifiedCode = 0; 
        user.UpdatedAt = DateTimeOffset.UtcNow;
 
        await _dbContext.SaveChangesAsync();
        
        var claims = new List<Claim>
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim("Email", user.Email),
            new Claim("Role", user.Role.ToString()),
        };
 
        if (user.Student != null)
            claims.Add(new Claim("studentId", user.Student.Id.ToString()));
 
        var accessToken = _jwtService.GenerateAccessToken(claims);
 
        return new Response.AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Fullname = $"{user.FirstName} {user.LastName}",
            Role = user.Role.ToString(),
            AccessToken = accessToken,
        };
    }

    public async Task<Response.AuthResponse> Login(Request.LoginRequest request)
    {
        var user = await _dbContext.Users
            .Include(u => u.Student)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            throw new KeyNotFoundException("Email hoặc mật khẩu không đúng.");

        if (!user.Verified)
            throw new InvalidOperationException("Tài khoản chưa được xác thực email.");

        if (user.PasswordHash != HashPassword(request.Password))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        var claims = new List<Claim>
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim("Email", user.Email),
            new Claim("Role", user.Role.ToString()),
        };
        
        var accessToken = _jwtService.GenerateAccessToken(claims);
        return new Response.AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Fullname = $"{user.FirstName} {user.LastName}",
            Role = user.Role.ToString(),
            AccessToken = accessToken,
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

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
    private static string BuildVerificationEmailBody(string fullName, int verifiedCode) => $"""
        <!DOCTYPE html>
        <html lang="vi">
        <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
        <body style='font-family:Arial,sans-serif;background:#f4f6f8;margin:0;padding:0;'>
            <table width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                    <td align='center' style='padding:40px 0;'>
                        <table width='600' cellpadding='0' cellspacing='0'
                               style='background:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08);'>
 
                            <!-- Header -->
                            <tr>
                                <td style='background:#4F46E5;padding:30px;text-align:center;'>
                                    <h1 style='color:#ffffff;margin:0;font-size:24px;'>SmartCenter</h1>
                                </td>
                            </tr>
 
                            <!-- Body -->
                            <tr>
                                <td style='padding:40px 30px;color:#333333;'>
                                    <p style='font-size:18px;'>Xin chào <strong>{fullName}</strong>,</p>
                                    <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>SmartCenter</strong>!</p>
                                    <p>Mã xác thực email của bạn là:</p>
 
                                    <div style='text-align:center;margin:36px 0;'>
                                        <span style='background:#f4f6f8;border:2px dashed #4F46E5;padding:16px 40px;
                                                     border-radius:8px;font-size:32px;font-weight:bold;
                                                     letter-spacing:8px;color:#4F46E5;'>
                                            {verifiedCode}
                                        </span>
                                    </div>
 
                                    <p style='color:#888;font-size:13px;'>
                                        Nhập mã này vào ứng dụng để xác thực tài khoản của bạn.
                                    </p>
                                    <p style='color:#888;font-size:13px;'>
                                        Nếu bạn không thực hiện đăng ký này, hãy bỏ qua email này.
                                    </p>
                                </td>
                            </tr>
 
                            <!-- Footer -->
                            <tr>
                                <td style='background:#f4f6f8;padding:20px;text-align:center;font-size:12px;color:#888;'>
                                    &copy; 2026 SmartCenter. All rights reserved.
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