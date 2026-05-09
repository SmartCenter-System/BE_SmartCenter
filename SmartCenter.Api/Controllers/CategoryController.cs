using Microsoft.AspNetCore.Mvc;
using SmartCenter.Service.CategoryService;
using SmartCenter.Service.Model;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController:ControllerBase
{
    private readonly IService _CategoryService;
    public CategoryController(IService categoryService)
    {
        _CategoryService = categoryService;
    }

    [HttpGet("get-categories")]
    public async Task<IActionResult> GetCategories()
    {
        return Ok(ApiResponseFactory.SuccessResponse(await _CategoryService.GetAllCategories(), "Lấy danh sách khóa học thành công", HttpContext.TraceIdentifier));
    }
}