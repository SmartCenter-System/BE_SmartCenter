namespace SmartCenter.Service.Auth;

public class Request
{
    public class RegisterRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? Phone { get; set; }
    }

    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? DeviceFingerprint { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public required string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        public required int Code { get; set; }
        public required string NewPassword { get; set; }
    }
    
    public class RegisterLecturerRequest
    { 
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? Phone { get; set; }
        
        public required string Bio { get; set; }
        public required string Expertise { get; set; }
    }
    
    public class RefreshTokenRequest
    {
        public required string RefreshToken { get; set; }
    }

    public class LogoutRequest
    {
        public required string RefreshToken { get; set; }
    }
    
    public class RegisterStaffRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? Phone { get; set; }
    }
}