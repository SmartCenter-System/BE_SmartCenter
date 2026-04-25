using SmartCenter.Repository.Abtraction;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Repository.Entity;

public class ConsultationRequest: BaseEntity<Guid>, IAuditableEntity
{
    
    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    public Guid CourseId { get; set; }
    public Course? Course { get; set; }
    
    public Guid? StaffId { get; set; }
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public string? Message { get; set; }
    public ConsultReqStatus Status { get; set; }   
    public string? Notes { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

}