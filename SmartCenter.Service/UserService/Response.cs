using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.UserService;

public class Response
{
    public class UserProfileResponse
    {
        public required string FirstName { get; set; } 
        public required string LastName { get; set; } 
        public required string Email { get; set; } 
        public string? Phone { get; set; }
        public required UserRole Role { get; set; }
        public string? ImgUrl { get; set; }

        public string? Address { get; set; }
        public string? City { get; set; }
        public string? ZaloLink { get; set; }

        public string? Bio { get; set; }
        public string? Expertise { get; set; }
    }
}