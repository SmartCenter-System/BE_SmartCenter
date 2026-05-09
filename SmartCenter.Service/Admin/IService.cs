using SmartCenter.Service.Base;

namespace SmartCenter.Service.Admin;

public interface IService
{
    Task<PagedResult<Response.UserItemResponse>>  GetUsersAsync(Request.GetUsersRequest request);
    Task<PagedResult<Response.OrderItemResponse>> GetOrdersAsync(Request.GetOrdersRequest request);
}