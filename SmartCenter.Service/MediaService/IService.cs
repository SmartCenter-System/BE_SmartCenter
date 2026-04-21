using Microsoft.AspNetCore.Http;

namespace SmartCenter.Service.MediaService;

public interface IService
{
    public Task<string> UploadImageAsync(IFormFile file);
}