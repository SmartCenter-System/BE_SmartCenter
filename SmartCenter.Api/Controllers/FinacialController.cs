using Microsoft.AspNetCore.Mvc;
using SmartCenter.Service.Financial;
using SmartCenter.Service.Model;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinacialController: ControllerBase
{
    private readonly IService _financialService;
 
    public FinacialController(IService financialService)
    {
        _financialService = financialService;
    }
 
    
    [HttpGet("transactions")]
    public async Task<IActionResult> GetAllTransactions([FromQuery] Request.FinancialFilterRequest request)
    {
        var result = await _financialService.GetAllTransactionsAsync(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Lấy danh sách giao dịch thành công!", HttpContext.TraceIdentifier));
    }
    
    [HttpGet("transactions/{orderId}")]
    public async Task<IActionResult> GetOrderDetail(Guid orderId)
    {
        var result = await _financialService.GetOrderDetailAsync(orderId);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Lấy chi tiết đơn hàng thành công!", HttpContext.TraceIdentifier));
    }
    
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueByPeriod([FromQuery] Request.RevenuePeriodRequest request)
    {
        var result = await _financialService.GetRevenueByPeriodAsync(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Lấy doanh thu theo kỳ thành công!", HttpContext.TraceIdentifier));
    }
    
    [HttpGet("revenue/courses")]
    public async Task<IActionResult> GetRevenuePerCourse()
    {
        var result = await _financialService.GetRevenuePerCourseAsync();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Lấy doanh thu theo khóa học thành công!", HttpContext.TraceIdentifier));
    }
}