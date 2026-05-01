using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.Order;

public class Service: IService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    
    private Guid GetStudentId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
            .FindFirst("studentId")?.Value;

        if (claim == null)
            throw new UnauthorizedAccessException("Not found information of student");

        return Guid.Parse(claim);
    }
    
    public async Task<Response.OrderResponse> CreateOrderAsync()
    {
        var studentId = GetStudentId();

        var cart = await _context.Carts
            .FirstOrDefaultAsync(x => x.StuId == studentId);
                   
        if(cart == null)
            throw new Exception("Cart not found");
        
        using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        try
        {
            var cartItems = await _context.CartItems
                .Include(x => x.Course)
                .Where(x => x.CartId == cart.Id)
                .ToListAsync();

            if (!cartItems.Any())
                throw new Exception("Cart is empty");

            foreach (var item in cartItems)
            {
                var enrolledCount = await _context.Enrollments
                    .CountAsync(e => e.CourseId == item.CourseId && e.Course.IsActive == true);

                if (enrolledCount >= item.Course!.MaxStudents)
                    throw new Exception($"Course {item.Course.CourseName} is full");
            }

            var order = new Repository.Entity.Order()
            {
                Id = Guid.NewGuid(),
                StuId = studentId,
                OrderCode = GenerateOrderCode(),
                Status = OrderStatus.Pending,
                PaymentMethod = PaymentMethod.BankTransfer,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpireAt = DateTimeOffset.UtcNow.AddMinutes(15),
                SubtotalAmount = 0,
                DiscountAmount = 0,
                TotalAmount = 0
            };

            _context.Orders.Add(order);

            decimal total = 0;

            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    CourseId = item.CourseId,
                    ItemName = item.Course!.CourseName,
                    UnitPrice = item.Course.BasePrice,
                    Quantity = 1,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                total += orderItem.UnitPrice * orderItem.Quantity;

                _context.OrderItems.Add(orderItem);
            }

            order.SubtotalAmount = total;
            order.TotalAmount = total;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new Response.OrderResponse
            {
                OrderId = order.Id,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                Items = cartItems.Select(x => new Response.OrderItemResponse
                {
                    CourseId = x.CourseId,
                    CourseTitle = x.Course!.CourseName,
                    UnitPrice = x.Course.BasePrice
                }).ToList()
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Response.OrderResponse?> GetOrderByIdAsync(Guid orderId)
    {
        var studentId = GetStudentId();
        
        return await _context.Orders
            .Where(o => o.Id == orderId && o.StuId == studentId)
            .Select(o => new Response.OrderResponse
            {
                OrderId = o.Id,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                Items = o.OrderItems!.Select(i => new Response.OrderItemResponse
                {
                    CourseId = i.CourseId,
                    CourseTitle = i.ItemName,
                    UnitPrice = i.UnitPrice
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    private string GenerateOrderCode()
    {
        return $"ORD{DateTime.UtcNow.Ticks}";
    }
    
    public async Task<List<Response.OrderResponse>> GetOrdersByUserAsync()
    {
        var studentId = GetStudentId();
        
        return await _context.Orders
            .Where(o => o.StuId == studentId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new Response.OrderResponse
            {
                OrderId = o.Id,
                OrderCode = o.OrderCode,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                ExpireAt = o.ExpireAt
            })
            .ToListAsync();
    }
    
    public async Task CancelOrderAsync(Guid orderId)
    {
        var studentId = GetStudentId();

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.StuId == studentId);
                        
        if(order == null)
            throw new Exception("Order not found");

        if (order.Status != OrderStatus.Pending)
            throw new Exception("Only pending orders can be cancelled");

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();
    }
}