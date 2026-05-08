namespace SmartCenter.Service.Combo;

public class Request
{
    public class CreateComboRequest
    {
        public required string Name { get; set; }
        public int DiscountPercent { get; set; }
        public List<Guid> CourseIds { get; set; } = new(); 
    }

    public class UpdateComboRequest
    {
        public string? Name { get; set; }
        public int? DiscountPercent { get; set; }
        public bool? IsActive { get; set; }
        public List<Guid>? CourseIds { get; set; }
    }
}