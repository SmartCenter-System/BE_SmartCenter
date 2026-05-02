using Microsoft.AspNetCore.Mvc;
using SmartCenter.Service.Auth;
using SmartCenter.Service.Model;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController: ControllerBase
{
    private readonly IService _identityService;
 
    public AuthController(IService identityService)
    {
        _identityService = identityService;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Request.RegisterRequest request)
    {
        var message = await _identityService.Register(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, message, HttpContext.TraceIdentifier));
    }
    
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] int code)
    {
        var result = await _identityService.VerifyEmail(code);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Xác thực email thành công!", HttpContext.TraceIdentifier));
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Request.LoginRequest request)
    {
        var result = await _identityService.Login(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Đăng nhập thành công!", HttpContext.TraceIdentifier));
    }
    
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] Request.ForgotPasswordRequest request)
    {
        var message = await _identityService.ForgotPassword(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, message, HttpContext.TraceIdentifier));
    }
    
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] Request.ResetPasswordRequest request)
    {
        var message = await _identityService.ResetPassword(request);
        return Ok(ApiResponseFactory.SuccessResponse(null, message, HttpContext.TraceIdentifier));
    }
}