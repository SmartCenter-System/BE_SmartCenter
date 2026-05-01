using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.Model;
using SmartCenter.Service.Order;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = JwtExtensions.StudentPolicy)]
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
            var result = await _orderService.CreateOrderAsync();

            return Ok(ApiResponseFactory.SuccessResponse(result, "Create Order successfully", HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
    
    [HttpGet("me/")]
    public async Task<IActionResult> GetOrdersByUser()
    {
        var result = await _orderService.GetOrdersByUserAsync();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get My Order successfully", HttpContext.TraceIdentifier));
    }
    
    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrderDetail(Guid orderId)
    {
        var result = await _orderService.GetOrderByIdAsync(orderId);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get Order Detail successfully", HttpContext.TraceIdentifier));
    }
    
    [HttpPut("{orderId}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid orderId)
    {
        try
        {
            await _orderService.CancelOrderAsync(orderId);
            return Ok(ApiResponseFactory.SuccessResponse(null, "Cancel Order successfully", HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}