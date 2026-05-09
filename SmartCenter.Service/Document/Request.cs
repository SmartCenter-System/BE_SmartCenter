using Microsoft.AspNetCore.Http;

namespace SmartCenter.Service.Document;

public class Request
{
    public class UploadDocumentRequest
    {
        public required IFormFile File { get; set; }
    }
}