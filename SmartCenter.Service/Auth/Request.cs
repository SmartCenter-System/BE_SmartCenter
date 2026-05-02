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
}