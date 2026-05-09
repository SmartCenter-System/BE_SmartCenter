using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Service.Base;

namespace SmartCenter.Service.Admin;

public class Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<Response.UserItemResponse>> GetUsersAsync(Request.GetUsersRequest request)
    {
        var query = _dbContext.Users.AsQueryable();

        if (request.Role.HasValue)
            query = query.Where(u => u.Role == request.Role.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(u => (u.FirstName + " " + u.LastName).ToLower().Contains(keyword)
                                  || u.Email.ToLower().Contains(keyword));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new Response.UserItemResponse
            {
                Id        = u.Id,
                FullName  = u.FirstName + " " + u.LastName,
                Email     = u.Email,
                Role      = u.Role.ToString(),
                Status    = u.Status.ToString(),
                CreatedAt = u.CreatedAt,
            })
            .ToListAsync();

        return new PagedResult<Response.UserItemResponse>
        {
            Items = items,
            Total = total,
        };
    }

    public async Task<PagedResult<Response.OrderItemResponse>> GetOrdersAsync(Request.GetOrdersRequest request)
    {
        var query = _dbContext.Orders
            .Include(o => o.Student)
                .ThenInclude(s => s.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Course)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(o => o.Status == request.Status.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new Response.OrderItemResponse
            {
                OrderId       = o.Id,
                OrderCode     = o.OrderCode,
                StudentName   = o.Student.User.FirstName + " " + o.Student.User.LastName,
                StudentEmail  = o.Student.User.Email,
                CourseNames   = o.OrderItems
                    .Where(oi => oi.Course != null)
                    .Select(oi => oi.Course!.CourseName)
                    .ToList(),
                TotalAmount   = o.TotalAmount,
                PaymentStatus = o.Status.ToString(),
                CreatedAt     = o.CreatedAt,
            })
            .ToListAsync();

        return new PagedResult<Response.OrderItemResponse>
        {
            Items = items,
            Total = total,
        };
    }
}