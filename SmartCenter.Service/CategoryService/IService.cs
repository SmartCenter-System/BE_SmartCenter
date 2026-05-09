namespace SmartCenter.Service.CategoryService;

public interface IService
{
    Task<List<Response.CategoryResponse>> GetAllCategories();
}