using SmartCenter.Service.Base;

namespace SmartCenter.Service.Course;

public interface IService
{
    Task<PagedResult<Response.CourseItemResponse>> GetCoursesAsync(Request.CourseFilterRequest request);
    Task<Response.CourseDetailResponse?> GetCourseDetailAsync(Guid courseId);
    Task<List<Response.LessonResponse>> GetCoursePreviewAsync(Guid courseId);
    
    Task<Response.CourseDetailResponse> CreateCourseAsync(Request.CreateCourseRequest request);
    Task<Response.CourseDetailResponse> UpdateCourseAsync(Guid courseId, Request.UpdateCourseRequest request);
    Task DeleteCourseAsync(Guid courseId);
    
    // public Task<Response.DashboardResponse> GetDashboard(Request.DashboardRequest request);
    Task<List<Response.CourseItemResponse>> GetTop6PopularCoursesAsync();
    Task<List<Response.RecentReviewResponse>> GetTop4HighRatedRecentReviewsAsync();

}