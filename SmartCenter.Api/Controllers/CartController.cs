using Microsoft.AspNetCore.Mvc;
using SmartCenter.Service.Cart;
using SmartCenter.Service.Model;

namespace SmartCenter.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CartController: ControllerBase
{
    private readonly IService _cartService;
    public CartController(IService cartService)
    {
        _cartService = cartService;
    }

    [HttpPost("create/{studentId}")]
    public async Task<IActionResult> CreateCart([FromRoute]Guid studentId)
    {
        try
        {
            await _cartService.CreateCart(studentId);
            return Ok(new { message = "Create Cart Successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet("{studentId}")]
    public async Task<IActionResult> GetCartItem([FromRoute] Guid studentId)
    {
        try
        {
            var items = await _cartService.GetCartItem(studentId);
            return Ok(items);
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
            return Ok(new { message = "Add to Cart Successfully" });
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
            return Ok(new { message = "Remove item from cart successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
}