using Microsoft.AspNetCore.Mvc;
using SmartCenter.Service.Order;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController: ControllerBase
{
    private readonly IService _orderService;

    public OrderController(IService orderService)
    {
        _orderService = orderService;
    }
    
    
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] Request.CreateOrderRequest request)
    {
        try
        {
            var result = await _orderService.CreateOrderAsync(request.StudentId, request.CartId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
    
    [HttpGet("user/{studentId}")]
    public async Task<IActionResult> GetOrdersByUser(Guid studentId)
    {
        var result = await _orderService.GetOrdersByUserAsync(studentId);
        return Ok(result);
    }
    
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id, [FromQuery] Guid studentId)
    {
        try
        {
            await _orderService.CancelOrderAsync(id, studentId);

            return Ok(new { message = "Order cancelled" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}