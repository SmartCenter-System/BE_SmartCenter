using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.Combo;
using SmartCenter.Service.Model;

namespace SmartCenter.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ComboController: ControllerBase
{
    private readonly IService _comboService;

    public ComboController(IService comboService)
    {
        _comboService = comboService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _comboService.GetAllCombosAsync();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get combos success", HttpContext.TraceIdentifier));
    }

    [HttpGet("{comboId}")]
    public async Task<IActionResult> GetComboById(Guid comboId)
    {
        var result = await _comboService.GetComboByIdAsync(comboId);
        if (result == null) return NotFound();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get combo success", HttpContext.TraceIdentifier));
    }
    
    [HttpPost]
    [Authorize(Policy = JwtExtensions.AdminOrLecturerPolicy)]
    public async Task<IActionResult> CreateCombo([FromBody] Request.CreateComboRequest request)
    {
        var result = await _comboService.CreateComboAsync(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Create combo success", HttpContext.TraceIdentifier));
    }

    [HttpPut("{comboId}")]
    [Authorize(Policy = JwtExtensions.AdminOrLecturerPolicy)]
    public async Task<IActionResult> UpdateCombo(Guid comboId, [FromBody] Request.UpdateComboRequest request)
    {
        var result = await _comboService.UpdateComboAsync(comboId, request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Update combo success", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{comboId:guid}")]
    [Authorize(Policy = JwtExtensions.AdminOrLecturerPolicy)]
    public async Task<IActionResult> DeleteCombo(Guid comboId)
    {
        await _comboService.DeleteComboAsync(comboId);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Delete combo success", HttpContext.TraceIdentifier));
    }
}