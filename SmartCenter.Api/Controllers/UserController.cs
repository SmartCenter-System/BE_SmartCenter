using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.Model;
using SmartCenter.Service.UserService;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IService _UserService;

    public UserController(IService userService)
    {
        _UserService = userService;
    }

    [HttpGet(template: "get-profile")]
    [Authorize(Policy = JwtExtensions.LectureOrStudentPolicy)]
    public async Task<IActionResult> GetProfile()
    {
        return Ok(ApiResponseFactory.SuccessResponse(await _UserService.GetProfileAsync(), "Get Profile Success",
            HttpContext.TraceIdentifier));
    }

    [HttpPost(template: "update-profile")]
    [Authorize(Policy = JwtExtensions.LectureOrStudentPolicy)]
    public async Task<IActionResult> UpdateProfile(Request.UpdateProfileRequest request)
    {
        return Ok(ApiResponseFactory.SuccessResponse(await _UserService.UpdateProfileAsync(request), "Update Profile Success", HttpContext.TraceIdentifier));
    }
}