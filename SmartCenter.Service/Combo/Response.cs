namespace SmartCenter.Service.Combo;

public class Response
{
    public class ComboResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DiscountPercent { get; set; }
        public bool IsActive { get; set; }
        public decimal OriginalPrice { get; set; }   
        public decimal DiscountedPrice { get; set; }
        public List<ComboItemResponse> Courses { get; set; } = new();
    }

    public class ComboItemResponse
    {
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
    }
}