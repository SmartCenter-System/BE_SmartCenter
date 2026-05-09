namespace SmartCenter.Service.Progress;

public interface IService
{
    Task MarkLessonCompleteAsync(Request.LessonProgressRequest request);
    Task<Response.CourseProgressResponse> GetCourseProgressAsync(Guid courseId);
}