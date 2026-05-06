using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.Cart;
using SmartCenter.Service.Model;

namespace SmartCenter.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = JwtExtensions.StudentPolicy)]
public class CartController: ControllerBase
{
    private readonly IService _cartService;
    public CartController(IService cartService)
    {
        _cartService = cartService;
    }

    [HttpPost("create/{studentId}")]
    public async Task<IActionResult> CreateCart()
    {
        try
        {
            var studentId = Guid.Parse(User.FindFirst("studentId")!.Value);
            await _cartService.CreateCart(studentId);
            return Ok(ApiResponseFactory.SuccessResponse(null, "Create Cart Successfully", HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet("{studentId}")]
    public async Task<IActionResult> GetCartItem()
    {
        try
        {
            var studentId = Guid.Parse(User.FindFirst("studentId")!.Value);
            var items = await _cartService.GetCartItem(studentId);
            return Ok(ApiResponseFactory.SuccessResponse(items, "Get Cart Successfully", HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPost("add")]
    public async Task<IActionResult> AddItemToCart([FromBody] Request.AddItemToCartRequest request)
    {
        try
        {
            await _cartService.AddItemToCart(request);
            return Ok(ApiResponseFactory.SuccessResponse(null, "Add Item to Cart Successfully", HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveItemFromCart([FromBody] Request.RemoveItemFromCartRequest request)
    {
        try
        {
            await _cartService.RemoveItemFromCart(request);
            return Ok(ApiResponseFactory.SuccessResponse(null, "Remove item from cart successfully", HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
}