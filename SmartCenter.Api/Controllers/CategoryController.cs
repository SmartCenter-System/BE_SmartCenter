using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Repository.Entity;
using SmartCenter.Service.CategoryService;
using SmartCenter.Service.Model;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly IService _CategoryService;

    public CategoryController(IService categoryService)
    {
        _CategoryService = categoryService;
    }

    [HttpGet("get-categories")]
    public async Task<IActionResult> GetCategories()
    {
        return Ok(ApiResponseFactory.SuccessResponse(await _CategoryService.GetAllCategories(),
            "Lấy danh sách khóa học thành công", HttpContext.TraceIdentifier));
    }

    [HttpPost("update-delete-categories")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> UpdateCategories([FromBody] Request.UpDateCategoryRequest request)
    {
        return Ok(ApiResponseFactory.SuccessResponse(await _CategoryService.UpDateCategory(request),
            "Cập nhật thành công", HttpContext.TraceIdentifier));
    }
    
    [HttpPost("create-categories")]
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    public async Task<IActionResult> CreateCategories([FromBody] Request.CreateCategoryRequest request)
    {
        return Ok(ApiResponseFactory.SuccessResponse(await _CategoryService.CreateCategory(request),
            "Tạo mới môn học thành công", HttpContext.TraceIdentifier));
    }
}