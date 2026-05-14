namespace SmartCenter.Service.Financial;

public interface IService
{
    Task<List<Response.TransactionResponse>> GetAllTransactionsAsync(Request.FinancialFilterRequest request);
    Task<Response.OrderDetailResponse> GetOrderDetailAsync(Guid orderId);
    Task<List<Response.RevenueByPeriodResponse>> GetRevenueByPeriodAsync(Request.RevenuePeriodRequest request);
    Task<List<Response.RevenuePerCourseResponse>> GetRevenuePerCourseAsync();

}