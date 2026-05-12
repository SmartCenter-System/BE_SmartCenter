using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.Admin;

public class Request
{
    public class GetUsersRequest
    {
        public UserRole? Role { get; set; }
        public string? Search { get; set; }    // tìm theo tên hoặc email
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetOrdersRequest
    {
        public OrderStatus? Status { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetUserDetailsRequest
    {
        public string UserId { get; set; }
    }
}