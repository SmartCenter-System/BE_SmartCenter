namespace SmartCenter.Service.UserService;

public class Request
{
    public class UpdateProfileRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? ImgUrl { get; set; }

        public string? Address { get; set; }
        public string? City { get; set; }
        public string? ZaloLink { get; set; }

        public string? Bio { get; set; }
        public string? Expertise { get; set; }
    }
}