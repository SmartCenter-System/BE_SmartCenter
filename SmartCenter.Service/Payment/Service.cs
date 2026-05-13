using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity;
using SmartCenter.Repository.Entity.Enums;
using SmartCenter.Service.SePayService;

namespace SmartCenter.Service.Payment;

public class Service : IService
{

    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private const string BankName = "MBBank";
    private const string BankAccount = "VQRQAIDXY6842";

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response.CreatePaymentResponse> CreatePaymentLinkAsync(Request.CreatePaymentRequest request)
{
    var studentId = _httpContextAccessor.HttpContext!.User
        .Claims.FirstOrDefault(x => x.Type == "studentId")?.Value;
    var studentIdGuid = Guid.Parse(studentId!);

    // 1. Validate course
    var course = await _dbContext.Courses
        .FirstOrDefaultAsync(c => c.Id == request.CourseId && c.IsActive)
        ?? throw new KeyNotFoundException("Không tìm thấy khóa học.");

    // 2. Kiểm tra đã mua chưa
    var alreadyEnrolled = await _dbContext.Enrollments
        .AnyAsync(e => e.CourseId == request.CourseId
                    && e.StuId   == studentIdGuid
                    && e.Status  == EnrollmentStatus.Paid);
    if (alreadyEnrolled)
        throw new InvalidOperationException("Bạn đã mua khóa học này rồi.");

    // 3. Dùng lại Order Pending còn hạn nếu có
    var existingOrder = await _dbContext.Orders
        .Include(o => o.OrderItems)
        .Where(o => o.StuId  == studentIdGuid
                 && o.Status == OrderStatus.Pending
                 && o.ExpireAt > DateTimeOffset.UtcNow)
        .FirstOrDefaultAsync(o => o.OrderItems.Any(oi => oi.CourseId == request.CourseId));

    Repository.Entity.Order order;

    if (existingOrder != null)
    {
        order = existingOrder;
    }
    else
    {
        // 4. Tạo Order mới
        order = new Repository.Entity.Order()
        {
            Id             = Guid.NewGuid(),
            StuId          = studentIdGuid,
            OrderCode      = $"ORD{DateTimeOffset.UtcNow.Ticks}",
            Status         = OrderStatus.Pending,
            PaymentMethod  = PaymentMethod.BankTransfer,
            SubtotalAmount = course.BasePrice,
            DiscountAmount = 0,
            TotalAmount    = course.BasePrice,
            CreatedAt      = DateTimeOffset.UtcNow,
            ExpireAt       = DateTimeOffset.UtcNow.AddMinutes(15),
        };
        _dbContext.Orders.Add(order);

        _dbContext.OrderItems.Add(new OrderItem
        {
            Id        = Guid.NewGuid(),
            OrderId   = order.Id,
            CourseId  = course.Id,
            ItemName  = course.CourseName,
            UnitPrice = course.BasePrice,
            Quantity  = 1,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await _dbContext.SaveChangesAsync();
    }

    // 5. Tạo QR
    var description = $"SMARTCENTER{order.Id.ToString("N")}";
    var qrCode = $"https://qr.sepay.vn/img?acc={BankAccount}" +
                 $"&bank={BankName}" +
                 $"&amount={(int)order.TotalAmount}" +
                 $"&des={Uri.EscapeDataString(description)}" +
                 $"&template=gronly";

    return new Response.CreatePaymentResponse
    {
        OrderId     = order.Id,
        OrderCode   = order.OrderCode,
        TotalAmount = order.TotalAmount,
        BankName    = BankName,
        BankAccount = BankAccount,
        Description = description,
        QRCode      = qrCode,
        ExpireAt    = order.ExpireAt,
    };
}


public async Task HandleWebhookAsync(Request.SepayWebhookRequest request)
    {
        var description =
            request.Content ??
            request.Description ??
            string.Empty;
        
        var match = Regex.Match(
            description,
            @"[a-fA-F0-9]{32}");
        
        if (!match.Success)
        {
            throw new Exception("Không tìm thấy GUID");
        }
        
        var raw = match.Value;
 
        Guid? orderId = null;
        
        if (raw.Length == 32)
        {
            var formatted = $"{raw.Substring(0, 8)}-" +
                            $"{raw.Substring(8, 4)}-" +
                            $"{raw.Substring(12, 4)}-" +
                            $"{raw.Substring(16, 4)}-" +
                            $"{raw.Substring(20, 12)}";

            if (Guid.TryParse(formatted, out var guid))
            {
                orderId = guid;
            }
        }
        else
        {
            throw new Exception("Định dạng mô tả không hợp lệ");
        }
        
        if (orderId == null)
        {
            throw new Exception("Không tìm thấy mã đơn hàng trong phần mô tả.");
        }
 
        var order = await _dbContext.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);
 
        if (order == null)
            throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
 
        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Đơn hàng đã được xử lý trước đó.");
 
        if (order.TotalAmount != request.TransferAmount)
            throw new ArgumentException("Số tiền chuyển khoản không khớp.");
 
        // Cập nhật Order -> Paid
        order.Status    = OrderStatus.Paid;
        order.PaidAt    = DateTimeOffset.UtcNow;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        
        var transaction = new Transaction()
        {
            Id                      = Guid.NewGuid(),
            OrderId                 = order.Id,
            Amount                  = request.TransferAmount,
            Status                  = "Full_Complete",
            ProviderTransactionCode = request.Id.ToString(),
            ConfirmedByStaffId      = Guid.Empty,
            ConfirmedAt             = DateTimeOffset.UtcNow,
            CreatedAt               = DateTimeOffset.UtcNow,
        };
        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync();
 
        // Tạo Enrollment cho từng course trong OrderItems
        var enrollments = order.OrderItems!
            .Where(o => o.CourseId.HasValue)
            .Select(o => new Enrollment
            {
                Id             = Guid.NewGuid(),
                StuId          = order.StuId,
                CourseId       = o.CourseId!.Value,
                TransactionId  = transaction.Id,
                EnrollmentDate = DateTimeOffset.UtcNow,
                Status         = EnrollmentStatus.Paid,
                CreatedAt      = DateTimeOffset.UtcNow,
            })
            .ToList();
 
        _dbContext.Enrollments.AddRange(enrollments);
        await _dbContext.SaveChangesAsync();
    }
}