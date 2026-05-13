using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity;

namespace SmartCenter.Service.CategoryService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Response.CategoryResponse>> GetAllCategories()
    {
        var query = _dbContext.Categories.Where(x => x.IsActive == true);

        var result = await query.Select(x => new Response.CategoryResponse()
        {
            Name = x.Name,
            Description = x.Description,
            IconUrl = x.IconUrl
        }).ToListAsync();
        return result;
    }

    public async Task<string> UpDateCategory(Request.UpDateCategoryRequest request)
    {
        var result = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Id == request.CategoryId);

        if (result == null)
        {
            throw new Exception("Category not found");
        }

        result.Id = request.CategoryId;

        if (request.CategoryName != null)
        {
            if (string.IsNullOrWhiteSpace(request.CategoryName))
                throw new Exception("Tên không được để trống");
            result.Name = request.CategoryName;
        }

        if (request.CategoryDescription != null)
        {
            if (string.IsNullOrWhiteSpace(request.CategoryDescription))
                throw new Exception("Mô tả không được để trống");
            result.Description = request.CategoryDescription;
        }

        if (request.CategoryIConUrl != null)
        {
            if (string.IsNullOrWhiteSpace(request.CategoryIConUrl))
                throw new Exception("IconUrl không được để trống");
            result.IconUrl = request.CategoryIConUrl;
        }

        result.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return "Cập nhật thành công";
    }

    public async Task<string> CreateCategory(Request.CreateCategoryRequest request)
    {
        var newCate = new Category()
        {
            IconUrl = request.CategoryIConUrl,
            Name = request.CategoryName,
            Description = request.CategoryDescription,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _dbContext.Categories.Add(newCate);
        await _dbContext.SaveChangesAsync();

        return "Tạo mới môn học thành công";
    }

    public async Task<string> DeleteCategory(Guid categoryId, bool isActive)
    {
        var cate = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Id == categoryId);

        if (cate == null)
        {
            throw new Exception("Category not found");
        }

        cate.IsActive = isActive;

        await _dbContext.SaveChangesAsync();

        return "Xóa thành công";
    }
}