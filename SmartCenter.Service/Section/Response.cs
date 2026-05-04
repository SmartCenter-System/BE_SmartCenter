namespace SmartCenter.Service.Section;

public class Response
{
    public class SectionResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}