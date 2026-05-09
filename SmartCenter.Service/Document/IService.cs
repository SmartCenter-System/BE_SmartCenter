namespace SmartCenter.Service.Document;

public interface IService
{
    Task<Response.DocumentResponse> UploadDocumentAsync(Guid lessonId, Request.UploadDocumentRequest request);
    Task<List<Response.DocumentResponse>> GetDocumentsByLessonAsync(Guid lessonId);
    Task DeleteDocumentAsync(Guid documentId);
}