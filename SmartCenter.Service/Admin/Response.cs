namespace SmartCenter.Service.Admin;

public class Response
{
    public class UserItemResponse
    {
        public Guid   Id        { get; set; }
        public string FullName  { get; set; } = string.Empty;
        public string Email     { get; set; } = string.Empty;
        public string Role      { get; set; } = string.Empty;
        public string Status    { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class OrderItemResponse
    {
        public Guid    OrderId       { get; set; }
        public string  OrderCode     { get; set; } = string.Empty;
        public string  StudentName   { get; set; } = string.Empty;
        public string  StudentEmail  { get; set; } = string.Empty;
        public List<string> CourseNames { get; set; } = new(); // 1 order nhiều course
        public decimal TotalAmount   { get; set; }
        public string  PaymentStatus { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
    
    public class DashBoardCourseResponse
    {
        public decimal totalRevenue { get; set; }
        public int totalStudents { get; set; }
        public int totalCourses { get; set; }
        public List<revenueChart> revenueCharts { get; set; }
    }

    public class revenueChart()
    {
     public string month { get; set; } 
     public decimal revenue { get; set; }  
    }
}