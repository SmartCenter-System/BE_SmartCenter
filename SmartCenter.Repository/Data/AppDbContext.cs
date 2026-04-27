using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCenter.Repository.Entity;

namespace SmartCenter.Repository.Data;

public class AppDbContext : DbContext
{
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Khai báo các bảng sẽ được tạo trong Database
    public DbSet<User> Users { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Lecturer> Lecturers { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<ReviewCourse> ReviewCourses { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<MultipleChoiceAnswer> MultipleChoiceAnswers { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<LearningProcess> LearningProcesses { get; set; }
    public DbSet<ExamPaperDetail> ExamPaperDetails { get; set; }
    public DbSet<ExamPaper> ExamPapers { get; set; }
    public DbSet<ExamManament> ExamManagements { get; set; }
    public DbSet<ExamManementDetail> ExamManagementDetails { get; set; }
    public DbSet<ExamComment> ExamComments { get; set; }
    public DbSet<EssayAnswer> EssayAnswers { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Deadline> Deadlines { get; set; }
    public DbSet<CourseCategory> CourseCategories { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<ComboCourse> ComboCourses { get; set; }
    public DbSet<Combo> Combos { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<ConsultationRequest> ConsultationRequests { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //=========================== User Configration =====================
       
    }
}