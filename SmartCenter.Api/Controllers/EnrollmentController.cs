using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Repository.Entity;
using SmartCenter.Service.EnrollmentService;

namespace SmartCenter.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class EnrollmentController:ControllerBase
{
    private readonly IService _enrollmentService;

    public EnrollmentController(IService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet(template: "MyEnrollments")]
    public async Task<IActionResult> GetMyEnrollments()
    {
        var result = _enrollmentService.GetMyEnrollment();
        return Ok(result);
    }

    [HttpPost(template: "")]
    public async Task<IActionResult> CreateEnrollment([FromBody] Request.EnrollmentRequest request)
    {
        var result = await _enrollmentService.CreateEnrollment(request);
        return Ok(result);
    }
}