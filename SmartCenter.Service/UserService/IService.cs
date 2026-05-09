namespace SmartCenter.Service.UserService;

public interface IService
{
    Task<Response.UserProfileResponse> GetProfileAsync();
    Task<Response.UserProfileResponse> UpdateProfileAsync(Request.UpdateProfileRequest request);
}