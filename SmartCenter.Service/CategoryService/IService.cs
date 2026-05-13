namespace SmartCenter.Service.CategoryService;

public interface IService
{
    Task<List<Response.CategoryResponse>> GetAllCategories();

    Task<String> UpDateCategory(Request.UpDateCategoryRequest request);
    
    Task<String> CreateCategory(Request.CreateCategoryRequest request);
    
    Task<String> DeleteCategory(Guid categoryId, bool isActive);
}