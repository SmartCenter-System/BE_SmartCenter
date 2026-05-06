using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.ConsultationService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<string> CreateConsultation(Request.ConsultationRequest request)
    {
        if (request.UserId != null)
        {
            var validUsers = await _dbContext.Users.Where(x => x.Id == request.UserId).FirstOrDefaultAsync();
            if (validUsers == null)
            {
                throw new Exception("User not found");
            }
        }

        var Consultation = new ConsultationRequest()
        {
            Email =  request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName =  request.LastName,
            Message =  request.Message,
            CourseId =  request.CourseId,
            UserId = request.UserId,
            RequestDate = DateTimeOffset.UtcNow,
            Status = ConsultReqStatus.Pending,
            Notes = request.Note,
        };

        _dbContext.ConsultationRequests.Add(Consultation);
        await _dbContext.SaveChangesAsync();
        return "Consultation Created successfully";
    }
}