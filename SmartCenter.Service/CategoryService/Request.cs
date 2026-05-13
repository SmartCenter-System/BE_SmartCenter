namespace SmartCenter.Service.CategoryService;

public class Request
{
    public class UpDateCategoryRequest()
    {
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryDescription { get; set; }
        public string? CategoryIConUrl { get; set; }
    }
    
    public class CreateCategoryRequest()
    {
        public required string CategoryName { get; set; }
        public required string CategoryDescription { get; set; }
        public required string CategoryIConUrl { get; set; }
    }
}