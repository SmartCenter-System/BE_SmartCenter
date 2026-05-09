using SmartCenter.Repository.Entity;

namespace SmartCenter.Service.ReviewCourseService;

public interface IService
{
    Task<Response.ReviewResponse> CreateReviewCourseAsync(Request.CreateReviewRequest request);
}