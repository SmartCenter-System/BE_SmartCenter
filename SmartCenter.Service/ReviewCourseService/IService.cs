using SmartCenter.Repository.Entity;

namespace SmartCenter.Service.ReviewCourseService;

public interface IService
{
    Task<Response.ReviewResponse> CreateReviewCourseAsync(Request.CreateReviewRequest request);

    Task<List<Response.ReviewDetailResponse>> GetReviewCourseAsync(Guid courseId, Guid? studentId);
}