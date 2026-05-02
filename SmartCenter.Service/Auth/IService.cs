

namespace SmartCenter.Service.Auth;

public interface IService
{
    Task<string> Register(Request.RegisterRequest request);
    Task<Response.AuthResponse> VerifyEmail(int code);
    Task<Response.AuthResponse> Login(Request.LoginRequest request);
    Task<string> ForgotPassword(Request.ForgotPasswordRequest request);
    Task<string>  ResetPassword(Request.ResetPasswordRequest request);
    
}