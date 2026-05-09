using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.Admin;
using SmartCenter.Service.Model;

namespace SmartCenter.Api.Controllers;
[ApiController]
[Route("api/admin")]
[Authorize(Policy = JwtExtensions.StaffOrAdminPolicy)]
public class AdminController: ControllerBase
{
    private readonly IService _adminService;

    public AdminController(IService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] Request.GetUsersRequest request)
    {
        var result = await _adminService.GetUsersAsync(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get users success", HttpContext.TraceIdentifier));
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders([FromQuery] Request.GetOrdersRequest request)
    {
        var result = await _adminService.GetOrdersAsync(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get orders success", HttpContext.TraceIdentifier));
    }
}