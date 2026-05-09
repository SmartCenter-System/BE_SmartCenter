using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;

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
        var query = _dbContext.Categories.Where(x => true);

        var result = await query.Select(x => new Response.CategoryResponse()
        {
            Name = x.Name,
            Description = x.Description,
            IconUrl = x.IconUrl
        }).ToListAsync();
        return result;
    }
}