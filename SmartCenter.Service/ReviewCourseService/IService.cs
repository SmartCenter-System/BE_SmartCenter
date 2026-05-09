using SmartCenter.Repository.Entity;
using SmartCenter.Service.Base;

namespace SmartCenter.Service.ReviewCourseService;

public interface IService
{
    Task<Response.ReviewResponse> CreateReviewCourseAsync(Request.CreateReviewRequest request);

    Task<List<Response.ReviewDetailResponse>> GetReviewCourseAsync(Guid courseId, Guid? studentId);
    Task<PagedResult<Response.ReviewAllResponse>> GetAllReviewsAsync(int pageIndex, int pageSize);
}