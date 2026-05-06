using SmartCenter.Repository.Abtraction;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Repository.Entity;

public class User: BaseEntity<Guid>, IAuditableEntity
{
    
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string? Phone { get; set; }
    public required UserRole Role { get; set; } //(Admin, Student, Lecturer, Staff
    public UserStatus Status { get; set; }
    public string? ImgUrl { get; set; }
    public bool Verified  { get; set; }
    public int VerifiedCode  { get; set; }
    public int ResetPasswordCode { get; set; }
    
    
    public ConsultationRequest? ConsultationRequest { get; set; }
    
    public Lecturer? Lecturer { get; set; }
    
    public Student? Student { get; set; }
    
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}