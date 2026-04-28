namespace SmartCenter.Service.Course;

public interface IService
{
    public Task<List<Response.CourseResponse>> GetAllCourses(string? search);
    
    public Task<Response.CourseResponse> GetCourseById(Guid id);
}