namespace SmartCenter.Service.Lesson;

public interface IService
{
    Task<List<Response.LessonResponse>> GetLessonsBySectionAsync(Guid sectionId);
    Task<Response.LessonResponse> CreateLessonAsync(Guid sectionId, Request.CreateLessonRequest request);
    Task<Response.LessonResponse> UpdateLessonAsync(Guid lessonId, Request.UpdateLessonRequest request);
    Task DeleteLessonAsync(Guid lessonId);
}