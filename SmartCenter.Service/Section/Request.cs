namespace SmartCenter.Service.Section;

public class Request
{
    public class CreateSectionRequest
    {
        public required string Title { get; set; }
        public int Position { get; set; }
    }

    public class UpdateSectionRequest
    {
        public string? Title { get; set; }
        public int? Position { get; set; }
    }
}