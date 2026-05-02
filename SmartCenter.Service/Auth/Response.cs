namespace SmartCenter.Service.Auth;

public class Response
{
    public class AuthResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
    }
}