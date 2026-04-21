using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SmartCenter.Repository.Data;

public class AppDbContext : DbContext
{
    
    
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Khai báo các bảng sẽ được tạo trong Database
    //public DbSet<User> Users { get; set; }
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //=========================== User Configration =====================
       
    }
}