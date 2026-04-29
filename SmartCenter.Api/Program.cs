using Microsoft.EntityFrameworkCore;
using Quartz;
using SmartCenter.Api.extensions;
using SmartCenter.Middlewares;
using SmartCenter.Repository.Data;

using JwtService = SmartCenter.Service.JwtService;
using MediaService = SmartCenter.Service.MediaService;
using CloudinaryService = SmartCenter.Service.CloudinaryService;
using MailService = SmartCenter.Service.MailService;
using SePayService = SmartCenter.Service.SePayService;
using CourseService = SmartCenter.Service.Course;
using CartService = SmartCenter.Service.Cart;
using OrderService = SmartCenter.Service.Order;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
builder.Services.AddJwtServices(builder.Configuration);
builder.Services.AddSwaggerServices();

builder.Services.AddScoped<JwtService.IJwtService, JwtService.JwtServices>();
builder.Services.AddScoped<MediaService.IService, CloudinaryService.Service>();
builder.Services.AddScoped<MailService.IService, MailService.Service>();
builder.Services.AddScoped<CourseService.IService, CourseService.Service>();
builder.Services.AddScoped<CartService.IService, CartService.Service>();
builder.Services.AddScoped<OrderService.IService, OrderService.Service>();

builder.Services.AddQuartz();

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();
var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();