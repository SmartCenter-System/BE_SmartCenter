using SmartCenter.Service.Base;

namespace SmartCenter.Service.Course;

public interface IService
{
    Task<PagedResult<Response.CourseItemResponse>> GetCoursesAsync(Request.CourseFilterRequest request);

    Task<Response.CourseDetailResponse?> GetCourseDetailAsync(Guid courseId);

    Task<List<Response.LessonResponse>> GetCoursePreviewAsync(Guid courseId);
}