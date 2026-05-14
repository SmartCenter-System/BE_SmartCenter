using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.Financial;

public class Service: IService
{
    private readonly AppDbContext _dbcontext;
    
    public Service(AppDbContext dbcontext)
    {
        _dbcontext = dbcontext;
    }
    
    public async Task<List<Response.TransactionResponse>> GetAllTransactionsAsync(Request.FinancialFilterRequest request)
    {
        var query = _dbcontext.Transactions
            .Include(t => t.Order)
            .ThenInclude(o => o.Student)
            .ThenInclude(s => s.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(t => t.Status == request.Status);

        if (request.StudentId.HasValue)
            query = query.Where(t => t.Order.StuId == request.StudentId.Value);
        
        if(request.FromDate.HasValue)
            query = query.Where(t => t.CreatedAt == request.FromDate.Value);
        
        if(request.ToDate.HasValue)
            query = query.Where(t => t.CreatedAt == request.ToDate.Value);

        return await query.OrderByDescending(t => t.CreatedAt)
            .Select(t => new Response.TransactionResponse
            {
                TransactionId = t.Id,
                OrderId = t.OrderId,
                OrderCode = t.Order.OrderCode,
                StudentName = t.Order.Student.User.FirstName + " " + t.Order.Student.User.LastName,
                StudentEmail = t.Order.Student.User.Email,
                Amount = t.Amount,
                Status = t.Status,
                ProviderTransactionCode = t.ProviderTransactionCode,
                ConfirmedAt = t.ConfirmedAt,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<Response.OrderDetailResponse> GetOrderDetailAsync(Guid orderId)
    {
        var order = await _dbcontext.Orders
            .Include(o => o.Student).ThenInclude(s => s.User)
            .Include(o => o.Items).ThenInclude(i => i.Course)
            .Include(o => o.Transaction)
            .FirstOrDefaultAsync(o => o.Id == orderId);
 
        if (order == null)
            throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
 
        return new Response.OrderDetailResponse
        {
            OrderId        = order.Id,
            OrderCode      = order.OrderCode,
            StudentName    = order.Student.User.FirstName + " " + order.Student.User.LastName,
            StudentEmail   = order.Student.User.Email,
            SubtotalAmount = order.SubtotalAmount,
            DiscountAmount = order.DiscountAmount,
            TotalAmount    = order.TotalAmount,
            Status         = order.Status.ToString(),
            PaidAt         = order.PaidAt,
            CreatedAt      = order.CreatedAt,
            Items = order.Items?.Select(i => new Response.OrderItemDetail
            {
                OrderItemId = i.Id,
                ItemName    = i.ItemName,
                UnitPrice   = i.UnitPrice,
                Quantity    = i.Quantity,
                CourseId    = i.CourseId,
            }).ToList() ?? new(),
            Transaction = order.Transaction == null ? null : new Response.TransactionDetail
            {
                TransactionId           = order.Transaction.Id,
                Amount                  = order.Transaction.Amount,
                Status                  = order.Transaction.Status,
                ProviderTransactionCode = order.Transaction.ProviderTransactionCode,
                ConfirmedAt             = order.Transaction.ConfirmedAt,
            }
        };
    }
    
    public async Task<List<Response.RevenueByPeriodResponse>> GetRevenueByPeriodAsync(Request.RevenuePeriodRequest request)
    {
        var query = _dbcontext.Transactions
            .Where(t => t.Status == "Completed")
            .AsQueryable();
 
        if (request.FromDate.HasValue)
            query = query.Where(t => t.ConfirmedAt >= request.FromDate.Value);
 
        if (request.ToDate.HasValue)
            query = query.Where(t => t.ConfirmedAt <= request.ToDate.Value);
        
        var transactions = await query
            .Select(t => new { t.Amount, t.ConfirmedAt })
            .ToListAsync();
 
        var grouped = request.Period.ToLower() switch
        {
            "day" => transactions
                .GroupBy(t => t.ConfirmedAt.ToString("yyyy-MM-dd"))
                .Select(g => new Response.RevenueByPeriodResponse
                {
                    PeriodLabel      = g.Key,
                    TotalRevenue     = g.Sum(t => t.Amount),
                    TransactionCount = g.Count(),
                }),
 
            "month" => transactions
                .GroupBy(t => t.ConfirmedAt.ToString("yyyy-MM"))
                .Select(g => new Response.RevenueByPeriodResponse
                {
                    PeriodLabel      = g.Key,
                    TotalRevenue     = g.Sum(t => t.Amount),
                    TransactionCount = g.Count(),
                }),
 
            "year" => transactions
                .GroupBy(t => t.ConfirmedAt.ToString("yyyy"))
                .Select(g => new Response.RevenueByPeriodResponse
                {
                    PeriodLabel      = g.Key,
                    TotalRevenue     = g.Sum(t => t.Amount),
                    TransactionCount = g.Count(),
                }),
 
            _ => throw new ArgumentException("Period không hợp lệ. Dùng: Day, Month, Year.")
        };
 
        return grouped.OrderBy(g => g.PeriodLabel).ToList();
    }
 
    public async Task<List<Response.RevenuePerCourseResponse>> GetRevenuePerCourseAsync()
    {
        return await _dbcontext.OrderItems
            .Where(i => i.CourseId.HasValue
                     && i.Order!.Status == OrderStatus.Paid)
            .GroupBy(i => new { i.CourseId, i.Course!.CourseName })
            .Select(g => new Response.RevenuePerCourseResponse
            {
                CourseId      = g.Key.CourseId!.Value,
                CourseName    = g.Key.CourseName,
                TotalRevenue  = g.Sum(i => i.UnitPrice * i.Quantity),
                TotalOrders   = g.Count(),
            })
            .OrderByDescending(r => r.TotalRevenue) 
            .ToListAsync();
    }
}