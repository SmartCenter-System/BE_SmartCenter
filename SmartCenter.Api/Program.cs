using Microsoft.EntityFrameworkCore;
using Quartz;
using SmartCenter.Api.extensions;
using SmartCenter.Middlewares;
using SmartCenter.Repository.Data;
using DotNetEnv;

using JwtService = SmartCenter.Service.JwtService;
using MediaService = SmartCenter.Service.MediaService;
using CloudinaryService = SmartCenter.Service.CloudinaryService;
using AuthService = SmartCenter.Service.Auth;
using MailService = SmartCenter.Service.MailService;
using SePayService = SmartCenter.Service.SePayService;
using CourseService = SmartCenter.Service.Course;
using CartService = SmartCenter.Service.Cart;
using OrderService = SmartCenter.Service.Order;
using ExamManagementService = SmartCenter.Service.ExamManagementService;
using GradeService = SmartCenter.Service.GradeService;
using ExamPaperService = SmartCenter.Service.ExamPaper;
using SectionService = SmartCenter.Service.Section;
using LessonService = SmartCenter.Service.Lesson;
using PaymentService = SmartCenter.Service.Payment;
using EnrollmentService = SmartCenter.Service.EnrollmentService;
using ConsultationService = SmartCenter.Service.ConsultationService;
using DocumentService = SmartCenter.Service.Document;
using ComboService = SmartCenter.Service.Combo;
using UserService = SmartCenter.Service.UserService;
using CommentService = SmartCenter.Service.Comment;
using ProgressService = SmartCenter.Service.Progress;
using ReviewCourseService = SmartCenter.Service.ReviewCourseService;
using CategoryService = SmartCenter.Service.CategoryService;

using AdminService = SmartCenter.Service.Admin;
using StaffService = SmartCenter.Service.Staff;


Env.Load();
var aspnetCoreEnv  = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", aspnetCoreEnv);



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
builder.Services.AddJwtServices(builder.Configuration);
builder.Services.AddSwaggerServices();

builder.Services.AddScoped<JwtService.IJwtService, JwtService.JwtServices>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<MediaService.IService, CloudinaryService.Service>();
builder.Services.AddScoped<MailService.IService, MailService.Service>();
builder.Services.AddScoped<CourseService.IService, CourseService.Service>();
builder.Services.AddScoped<CartService.IService, CartService.Service>();
builder.Services.AddScoped<OrderService.IService, OrderService.Service>();
builder.Services.AddScoped<AuthService.IService, AuthService.Service>();
builder.Services.AddScoped<EnrollmentService.IService, EnrollmentService.Service>();
builder.Services.AddScoped<ConsultationService.IService, ConsultationService.Service>();
builder.Services.AddScoped<ExamManagementService.IService, ExamManagementService.Service>();
builder.Services.AddScoped<GradeService.IService, GradeService.Service>();
builder.Services.AddScoped<ExamPaperService.IService, ExamPaperService.Service>();
builder.Services.AddScoped<PaymentService.IService, PaymentService.Service>();
builder.Services.AddScoped<LessonService.IService, LessonService.Service>();
builder.Services.AddScoped<SectionService.IService, SectionService.Service>();
builder.Services.AddScoped<DocumentService.IService, DocumentService.Service>();
builder.Services.AddScoped<ComboService.IService, ComboService.Service>();

builder.Services.AddScoped<UserService.IService, UserService.Service>();

builder.Services.AddScoped<CommentService.IService, CommentService.Service>();
builder.Services.AddScoped<ProgressService.IService, ProgressService.Service>();
builder.Services.AddScoped<ReviewCourseService.IService, ReviewCourseService.Service>();
builder.Services.AddScoped<CategoryService.IService, CategoryService.Service>();
builder.Services.AddScoped<AdminService.IService, AdminService.Service>();
builder.Services.AddScoped<StaffService.IService, StaffService.Service>();



// ─── Quartz ───────────────────────────────────────────────────────────────────
builder.Services.AddQuartz();
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

// ─── Middleware ────────────────────────────────────────────────────────────────
builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await AppDbContextSeed.SeedAsync(db);
}

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// app.UseSwagger();
app.UseSwaggerAPI();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();