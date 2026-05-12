using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Repository.Entity;
using SmartCenter.Service.EnrollmentService;
using SmartCenter.Service.Model;

namespace SmartCenter.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EnrollmentController : ControllerBase
{
    private readonly IService _enrollmentService;

    public EnrollmentController(IService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet(template: "my-enrollments")]
    public async Task<IActionResult> GetMyEnrollments()
    {
        return Ok(ApiResponseFactory.SuccessResponse(
            data: await _enrollmentService.GetMyEnrollment(),
            message: "Lấy danh sách khóa học đã đăng ký thành công.",
            traceId: HttpContext.TraceIdentifier
        ));
    }

    [HttpPost(template: "create-enrollment")]
    public async Task<IActionResult> CreateEnrollment([FromBody] Request.EnrollmentRequest request)
    {
        return Ok(ApiResponseFactory.SuccessResponse(
            data: await _enrollmentService.CreateEnrollment(request),
            message: "Đăng ký khóa học thành công.",
            HttpContext.TraceIdentifier
        ));
    }
}