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

    public async Task<Response.CreatePaymentResponse> CreatePaymentLinkAsync(Guid orderId)
    {
        var studentId = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(x => x.Type == "studentId")?.Value;
        var studentIdGuid = Guid.Parse(studentId!);

        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.StuId == studentIdGuid);

        if (order == null)
            throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Đơn hàng không ở trạng thái chờ thanh toán.");
        
        var description = $"SMARTCENTER - {order.Id}";

        var qrCode = $"https://qr.sepay.vn/img?acc={BankAccount}" +
                     $"&bank={BankName}" +
                     $"&amount={(int)order.TotalAmount}" +
                     $"&des={description}" +
                     $"&template=gronly";

        return new Response.CreatePaymentResponse
        {
            OrderId = order.Id,
            OrderCode = order.OrderCode,
            TotalAmount = order.TotalAmount,
            BankName = BankName,
            BankAccount = BankAccount,
            Description = description,
            QRCode = qrCode
        };
    }


public async Task HandleWebhookAsync(Request.SepayWebhookRequest request)
    {
        var description = request.Code ?? string.Empty;
        var raw  = description.Replace("SMARTCENTER", "").Trim();
 
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
            .Include(o => o.Items)
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
            Status                  = "Completed",
            ProviderTransactionCode = request.Id ?? Guid.NewGuid().ToString(),
            ConfirmedByStaffId      = Guid.Empty,
            ConfirmedAt             = DateTimeOffset.UtcNow,
            CreatedAt               = DateTimeOffset.UtcNow,
        };
        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync();
 
        // Tạo Enrollment cho từng course trong OrderItems
        var enrollments = order.Items
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