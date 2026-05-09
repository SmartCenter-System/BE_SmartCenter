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
        public string  RefreshToken { get; set; } = string.Empty;
        
        public string Phone { get; set; } = string.Empty;
    }
    
    public class LecturerRegisterResponse
    {
        public Guid UserId { get; set; }
        public Guid LecturerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Expertise { get; set; } = string.Empty;
    }
}