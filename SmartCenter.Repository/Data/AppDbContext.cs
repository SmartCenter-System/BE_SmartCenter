using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCenter.Repository.Entity;
using SmartCenter.Repository.Entity.Enums;

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
        modelBuilder.Entity<User>(builder =>
        {
            builder.Property(u => u.FirstName).IsRequired().HasMaxLength(128);
            builder.Property(u => u.LastName).IsRequired().HasMaxLength(128);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(128);
            builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(128);
            builder.Property(u => u.Phone).IsRequired().HasMaxLength(15);
            builder.Property(u => u.Role).IsRequired().HasDefaultValue(UserRole.Student);
            builder.Property(u => u.Status).IsRequired().HasDefaultValue(UserStatus.Active);
            builder.Property(u => u.Verified).IsRequired();
            builder.Property(u => u.ImgUrl).HasMaxLength(500);
            builder.Property(u => u.VerifiedCode).IsRequired();
            
            
            builder.HasMany(u => u.Comments).WithOne(c => c.User).HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(u => u.ConsultationRequest).WithOne(c => c.User).HasForeignKey<ConsultationRequest>(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(u => u.AuditLogs).WithOne(a => a.User).HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(u => u.Notifications).WithOne(c => c.User).HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(u => u.Lecturer).WithOne(l => l.User).HasForeignKey<Lecturer>(l => l.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(u => u.Student).WithOne(s => s.User).HasForeignKey<Student>(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<UserSession>( builder => 
        {
            builder.Property(us => us.DeviceFingerprint).IsRequired().HasMaxLength(300);
            builder.Property(s => s.RefreshToken).IsRequired().HasMaxLength(512);
            builder.Property(s => s.ExpiresAt).IsRequired();
            builder.Property(s => s.IsRevoked).HasDefaultValue(false);
            
            builder.HasIndex(s => s.UserId);
            builder.HasIndex(s => s.RefreshToken).IsUnique();
            
            builder.HasOne<User>()
                .WithMany() 
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Comment>(builder =>
        {
            builder.Property(c => c.Content).IsRequired().HasMaxLength(500);
            builder.Property(c => c.DepthLevel).IsRequired();
            builder.Property(c => c.IsLocked).IsRequired();
            
            builder.HasMany(c => c.Replies).WithOne(c => c.ParentComment).HasForeignKey(c => c.ParentCommentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConsultationRequest>(builder =>
        {
            builder.Property(u => u.FirstName).IsRequired().HasMaxLength(128);
            builder.Property(u => u.LastName).IsRequired().HasMaxLength(128);
            builder.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(15);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(128);
            builder.Property(u => u.Message).HasMaxLength(500);
            builder.Property(u => u.RequestDate).IsRequired().HasDefaultValue(DateTimeOffset.UtcNow);
            builder.Property(u => u.Status).IsRequired().HasDefaultValue(ConsultReqStatus.Pending);
            builder.Property(u => u.Notes).HasMaxLength(500);
        });

        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.Property(e => e.Action).IsRequired();
            builder.Property(e => e.Entity).IsRequired().HasMaxLength(200);
            builder.Property(e => e.EntityId).IsRequired();
            builder.Property(e => e.Timestamp).HasDefaultValueSql("now()");
            builder.Property(e => e.Metadata).HasColumnType("jsonb");
            
        });

        modelBuilder.Entity<Notification>(builder =>
        {
            builder.Property(n => n.Title).IsRequired().HasMaxLength(128);
            builder.Property(n => n.Description).IsRequired().HasMaxLength(500);
            builder.Property(n => n.Type).IsRequired();
            builder.Property(n => n.RefId).HasMaxLength(50);
            builder.Property(n => n.RefType).HasMaxLength(50);

            builder.Property(n => n.IsRead).HasDefaultValue(false);
            builder.Property(n => n.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Lecturer>(builder =>
        {
            builder.Property(l => l.Bio);
            builder.Property(l => l.Expertise).IsRequired();
            
            builder.HasMany(l => l.ExamPapers).WithOne(l => l.Lecturer).HasForeignKey(l => l.LecturerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(l => l.Courses).WithOne(l => l.Lecturer).HasForeignKey(l => l.LecId).OnDelete(DeleteBehavior.Restrict);
            
        });

        modelBuilder.Entity<Student>(builder =>
        {
            builder.Property(s => s.Address).IsRequired().HasMaxLength(500);
            builder.Property(s => s.City).IsRequired().HasMaxLength(50);
            builder.Property(s => s.EnrollmentDate);
            builder.Property(s => s.ZaloLink).HasMaxLength(500);
            
            builder.HasMany(s => s.Orders).WithOne(s => s.Student).HasForeignKey(s => s.StuId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(s => s.Enrollments).WithOne(s => s.Student).HasForeignKey(s => s.StuId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(s => s.Cart).WithOne(s => s.Student).HasForeignKey<Cart>(s => s.StuId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(s => s.ExamManaments).WithOne(s => s.Student).HasForeignKey(s => s.StudentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(s => s.LearningProcesses).WithOne(s => s.Student).HasForeignKey(s => s.StuId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(s => s.ReviewCourses).WithOne(s => s.Student).HasForeignKey(s => s.StuId).OnDelete(DeleteBehavior.Restrict);
        });
        
        
        modelBuilder.Entity<CartItem>(builder =>
        {
            builder.Property(ci => ci.Quantity).IsRequired();
            builder.Property(ci => ci.ItemType).IsRequired();//Course, Combo
            
            builder.HasOne(c => c.Cart).WithMany(c => c.Items).HasForeignKey(c => c.CartId).OnDelete(DeleteBehavior.Cascade);

        });
        
        modelBuilder.Entity<ExamPaper>(builder =>
        {
            builder.Property(e => e.Title).IsRequired().HasMaxLength(500);
            builder.Property(e => e.CountDown).IsRequired();
            builder.Property(e => e.TotalPoints).IsRequired();
            builder.Property(e => e.Status).IsRequired().HasDefaultValue(ExamPaperStatus.Open);
            
            builder.HasMany(e => e.ExamManaments).WithOne(e => e.ExamPaper).HasForeignKey(e => e.ExamPaperId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(e => e.ExamComments).WithOne(e => e.ExamPaper).HasForeignKey(e => e.ExamPaperId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(e => e.ExamPaperDetails).WithOne(e => e.ExamPaper).HasForeignKey(e => e.ExamPaperId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Deadline).WithOne(e => e.ExamPaper).HasForeignKey<Deadline>(e => e.ExamPaperId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Deadline>(builder =>
        {
            builder.Property(d => d.Title).HasMaxLength(255).IsRequired();
            builder.Property(d => d.EndedAt).IsRequired();

            builder.Property(d => d.Status).IsRequired().HasDefaultValue(DeadlineStatus.Processing);
        });
        
        modelBuilder.Entity<ExamManament>(builder =>
        {
            builder.Property(e => e.PointsOfStudent).IsRequired();
            
            builder.HasMany(e => e.ExamManementDetails).WithOne(e => e.ExamManement).HasForeignKey(e => e.ExamManementId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExamComment>(builder =>
        {
            builder.Property(e => e.Content).IsRequired().HasMaxLength(500);
            builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValue(DateTimeOffset.UtcNow);
            builder.Property(e => e.NumberOfLikes).IsRequired().HasDefaultValue(0);
            builder.Property(e => e.NumberOfDislikes).IsRequired().HasDefaultValue(0);
            
            builder.HasMany(e => e.Comments).WithOne(e => e.ParentExamComment).HasForeignKey(e => e.ParentExamCommentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExamPaperDetail>(builder =>
        {
            builder.HasMany(e => e.ExamManementDetails).WithOne(e => e.ExamPaperDetail).HasForeignKey(e => e.ExamPaperDetailId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Question>(builder =>
        {
            builder.Property(q => q.Title).IsRequired().HasMaxLength(500);
            builder.Property(q => q.TypeOfQuestion).IsRequired();
            builder.Property(q => q.Point).IsRequired();
            
            builder.HasMany(q => q.ExamPaperDetails).WithOne(e => e.Question).HasForeignKey(e => e.QuestionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(q => q.MultipleChoiceAnswers).WithOne(e => e.Question).HasForeignKey(e => e.QuestionId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(q => q.EssayAnswers).WithOne(e => e.Question).HasForeignKey(e => e.QuestionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EssayAnswer>(builder =>
        {
            builder.Property(e => e.Content).IsRequired().HasMaxLength(500);
        });

        modelBuilder.Entity<MultipleChoiceAnswer>(builder =>
        {
            builder.Property(e => e.Content).IsRequired().HasMaxLength(500);
            builder.Property(e => e.IsCorrect).IsRequired();
            
            builder.HasMany(e => e.ExamManementDetails).WithOne(e => e.MultipleChoiceAnswer).HasForeignKey(e => e.MultipleChoiceAnswerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ExamManementDetail>(builder =>
        {
            builder.Property(e => e.Answer).IsRequired().HasMaxLength(500);
            builder.Property(e => e.Point).IsRequired();
            builder.Property(e => e.IsMultiChoice).IsRequired();
            builder.Property(e => e.Feedback).HasMaxLength(500);
        });

        modelBuilder.Entity<Course>(builder =>
        {
            builder.Property(c => c.CourseName).IsRequired().HasMaxLength(500);
            builder.Property(c => c.Description).IsRequired();
            builder.Property(c => c.CourseType).IsRequired();
            builder.Property(c => c.BasePrice).IsRequired();
            builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(c => c.ImgUrl).IsRequired().HasMaxLength(500);
            builder.Property(c => c.StartAt).IsRequired();
            builder.Property(c => c.EndAt).IsRequired();
            builder.Property(c => c.MaxStudents).IsRequired();
            builder.Property(c => c.AcademicYear).IsRequired().HasDefaultValue(DateTime.Now.Year);
            
            builder.HasMany(c => c.ComboCourses).WithOne(c => c.Course).HasForeignKey(c => c.CourseId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.Enrollments).WithOne(c => c.Course).HasForeignKey(c => c.CourseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(c => c.Sections).WithOne(c => c.Course).HasForeignKey(c => c.CourseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(c => c.Reviews).WithOne(c => c.Course).HasForeignKey(c => c.CourseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(c => c.ConsultationRequests).WithOne(c => c.Course).HasForeignKey(c => c.CourseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(c => c.CourseCategories).WithOne(c => c.Course).HasForeignKey(c => c.CourseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(c => c.Lessons).WithOne(c => c.Course).HasForeignKey(c => c.CourseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(c => c.CartItems).WithOne(c => c.Course).HasForeignKey(c => c.CourseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(c => c.OrderItems).WithOne(c => c.Course).HasForeignKey(c => c.CourseId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReviewCourse>(builder =>
        {
            builder.Property(c => c.Rating).IsRequired().HasDefaultValue(0);
            builder.Property(c => c.Comment).IsRequired().HasMaxLength(500);
            builder.Property(c => c.CreatedAt).IsRequired().HasDefaultValue(DateTimeOffset.UtcNow);
        });

        modelBuilder.Entity<Section>(builder =>
        {
            builder.Property(s => s.Title).IsRequired().HasMaxLength(500);
            builder.Property(s => s.Position).IsRequired().HasMaxLength(100);
            builder.Property(s => s.IsActive).IsRequired().HasDefaultValue(true);
            
            builder.HasMany(s => s.Lessons).WithOne(s => s.Section).HasForeignKey(s => s.SectionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Enrollment>(builder =>
        {
            builder.Property(e => e.EnrollmentDate).IsRequired();
            builder.Property(e => e.Status).IsRequired();//Paid / Deposit
        });

        modelBuilder.Entity<Order>(builder =>
        {
            builder.Property(o => o.OrderCode).IsRequired().HasMaxLength(50);
            builder.Property(o => o.SubtotalAmount).IsRequired().HasPrecision(18, 3);
            builder.Property(o => o.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0m);
            builder.Property(o => o.TotalAmount).IsRequired().HasPrecision(18, 3);
            
            builder.Property(o => o.Status).IsRequired().HasDefaultValue(OrderStatus.Pending);
            builder.Property(o => o.PaymentMethod).IsRequired();
            builder.Property(o => o.Note).IsRequired(false).HasMaxLength(500);
            builder.Property(o => o.ExpireAt).IsRequired();
            builder.Property(o => o.PaidAt).IsRequired(false);
            
            builder.HasMany(o => o.OrderItems).WithOne(o => o.Order).HasForeignKey(o => o.OrderId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(o => o.Transaction).WithOne(o => o.Order).HasForeignKey<Transaction>(o => o.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(o => o.OrderCode).IsUnique();
        });

        modelBuilder.Entity<OrderItem>(builder =>
        {
            builder.Property(oi => oi.ItemName).IsRequired().HasMaxLength(255);

            builder.Property(oi => oi.UnitPrice).HasPrecision(18, 3).IsRequired();

            builder.Property(oi => oi.Quantity).IsRequired().HasDefaultValue(1);
        });
        
        modelBuilder.Entity<Transaction>(builder =>
        {
            builder.Property(t => t.Amount).IsRequired();
            builder.Property(t => t.Status).IsRequired();//Part_Complete / Full_Complete
            builder.Property(t => t.ProviderTransactionCode).IsRequired();
            builder.Property(t => t.ConfirmedByStaffId).IsRequired();
            builder.Property(t => t.ConfirmedAt).IsRequired().HasDefaultValue(DateTimeOffset.UtcNow);
            
            builder.HasMany(t => t.Enrollments).WithOne(t => t.Transaction).HasForeignKey(t => t.TransactionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Combo>(builder =>
        {
            builder.Property(c => c.Name).IsRequired().HasMaxLength(500);
            builder.Property(c => c.DiscountPercent).IsRequired();
            builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);

            builder.HasMany(c => c.ComboCourses).WithOne(c => c.Combo).HasForeignKey(c => c.ComboId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.OrderItems).WithOne(c => c.Combo).HasForeignKey(c => c.ComboId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(c => c.CartItems).WithOne(c => c.Combo).HasForeignKey(c => c.ComboId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Lesson>(builder =>
        {
            builder.Property(l => l.Description).IsRequired();
            builder.Property(l => l.VideoUrl).HasMaxLength(500);
            builder.Property(l => l.Duration).IsRequired();
            builder.Property(l => l.IsPreview).HasDefaultValue(false);
            builder.Property(l => l.Position).IsRequired();
            
            builder.HasMany(l => l.Comments).WithOne(l => l.Lesson).HasForeignKey(l => l.LessonId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(l => l.Documents).WithOne(l => l.Lesson).HasForeignKey(l => l.LessonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(l => l.LearningProcesses).WithOne(l => l.Lesson).HasForeignKey(l => l.LessonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(l => l.ExamPapers).WithOne(l => l.Lesson).HasForeignKey(l => l.LessonId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Document>(builder =>
        {
            builder.Property(d => d.FileName).HasMaxLength(500);
            builder.Property(d => d. FileUrl).HasMaxLength(500);
            builder.Property(d => d.FileType).IsRequired();
        });

        modelBuilder.Entity<LearningProcess>(builder =>
        {
            builder.Property(lp => lp.WatchTime).IsRequired().HasDefaultValue(0);
            builder.Property(l => l.IsCompleted).IsRequired().HasDefaultValue(false);
            builder.Property(l => l.LastWatchedAt).IsRequired().HasDefaultValueSql("now()");
            
            builder.HasIndex(lp => new { lp.StuId, lp.LessonId }).IsUnique();
        });

        modelBuilder.Entity<Category>(builder =>
        {
            builder.Property(c => c.Name).IsRequired().HasMaxLength(500);
            builder.Property(c => c.Description).IsRequired();
            builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(c => c.IconUrl).IsRequired();
            
            builder.HasMany(c => c.CourseCategories).WithOne(c => c.Category).HasForeignKey(c => c.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}