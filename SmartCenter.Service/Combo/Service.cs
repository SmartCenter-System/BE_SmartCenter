using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity;

namespace SmartCenter.Service.Combo;

public class Service: IService
{
    
    private readonly AppDbContext _dbContext;
    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<List<Response.ComboResponse>> GetAllCombosAsync()
    {
        var query = _dbContext.Combos
            .Where(c => c.IsActive)
            .Include(c => c.ComboCourses)
            .ThenInclude(cc => cc.Course)
            .Select(c => MapResponse(c));
        var combos = await query.ToListAsync();
        return combos;
    }

    public async Task<Response.ComboResponse?> GetComboByIdAsync(Guid comboId)
    {
        var query = await _dbContext.Combos
            .Include(c => c.ComboCourses)
            .ThenInclude(cc => cc.Course)
            .FirstOrDefaultAsync(c => c.Id == comboId);

        if(query == null)
            throw new Exception("Không tìm thấy combo");
        
        var combo = MapResponse(query);
        return combo;
    }

    public async Task<Response.ComboResponse> CreateComboAsync(Request.CreateComboRequest request)
    {
        var courses = await _dbContext.Courses
            .Where(c => request.CourseIds.Contains(c.Id) && c.IsActive)
            .ToListAsync();

        if (courses.Count != request.CourseIds.Count)
            throw new Exception("Một hoặc nhiều khóa học không tồn tại hoặc đã ngừng hoạt động.");

        var combo = new Repository.Entity.Combo
        {
            Id              = Guid.NewGuid(),
            Name            = request.Name,
            DiscountPercent = request.DiscountPercent,
            IsActive        = true,
            CreatedAt       = DateTimeOffset.UtcNow,
        };
        _dbContext.Combos.Add(combo);

        var comboCourses = courses.Select(c => new ComboCourse()
        {
            Id       = Guid.NewGuid(),
            ComboId  = combo.Id,
            CourseId = c.Id,
            CreatedAt = DateTimeOffset.UtcNow,
        }).ToList();
        _dbContext.ComboCourses.AddRange(comboCourses);

        await _dbContext.SaveChangesAsync();
        return (await GetComboByIdAsync(combo.Id))!;
    }

    public async Task<Response.ComboResponse> UpdateComboAsync(Guid comboId, Request.UpdateComboRequest request)
    {
        var combo = await _dbContext.Combos
            .Include(c => c.ComboCourses)
            .FirstOrDefaultAsync(c => c.Id == comboId)
        ?? throw new Exception("Không tìm thấy combo.");

        if (request.Name            != null) combo.Name            = request.Name;
        if (request.DiscountPercent != null) combo.DiscountPercent = request.DiscountPercent.Value;
        if (request.IsActive        != null) combo.IsActive        = request.IsActive.Value;
        combo.UpdatedAt = DateTimeOffset.UtcNow;

        if (request.CourseIds != null)
        {
            var courses = await _dbContext.Courses
                .Where(c => request.CourseIds.Contains(c.Id) && c.IsActive)
                .ToListAsync();

            if (courses.Count != request.CourseIds.Count)
                throw new Exception("Một hoặc nhiều khóa học không tồn tại hoặc đã ngừng hoạt động.");

            _dbContext.ComboCourses.RemoveRange(combo.ComboCourses);
            var newComboCourses = courses.Select(c => new ComboCourse
            {
                Id        = Guid.NewGuid(),
                ComboId   = combo.Id,
                CourseId  = c.Id,
                CreatedAt = DateTimeOffset.UtcNow,
            }).ToList();
            _dbContext.ComboCourses.AddRange(newComboCourses);
        }

        await _dbContext.SaveChangesAsync();
        return (await GetComboByIdAsync(comboId))!;
    }

    public async Task DeleteComboAsync(Guid comboId)
    {
        var combo = await _dbContext.Combos.FindAsync(comboId)
            ?? throw new Exception("Không tìm thấy combo.");

        combo.IsActive  = false;
        combo.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();
    }
    
    private static Response.ComboResponse MapResponse(Repository.Entity.Combo c)
    {
        var originalPrice   = c.ComboCourses.Sum(cc => cc.Course.BasePrice);
        var discountedPrice = originalPrice * (1 - c.DiscountPercent / 100m);

        return new Response.ComboResponse
        {
            Id              = c.Id,
            Name            = c.Name,
            DiscountPercent = c.DiscountPercent,
            IsActive        = c.IsActive,
            OriginalPrice   = originalPrice,
            DiscountedPrice = discountedPrice,
            Courses         = c.ComboCourses.Select(cc => new Response.ComboItemResponse
            {
                CourseId   = cc.CourseId,
                CourseName = cc.Course.CourseName,
                BasePrice  = cc.Course.BasePrice,
            }).ToList(),
        };
    }
}