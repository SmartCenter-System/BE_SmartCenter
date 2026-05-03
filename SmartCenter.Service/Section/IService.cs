namespace SmartCenter.Service.Section;

public interface IService
{
    Task<List<Response.SectionResponse>> GetSectionsByCourseAsync(Guid courseId);
    Task<Response.SectionResponse> CreateSectionAsync(Guid courseId, Request.CreateSectionRequest request);
    Task<Response.SectionResponse> UpdateSectionAsync(Guid sectionId, Request.UpdateSectionRequest request);
    Task DeleteSectionAsync(Guid sectionId);
}