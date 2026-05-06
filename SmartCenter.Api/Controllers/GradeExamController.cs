using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCenter.Api.extensions;
using SmartCenter.Service.GradeService;

namespace SmartCenter.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class GradeExamController:ControllerBase
{
    private readonly IService _gradeService;

    public GradeExamController(IService gradeService)
    {
        _gradeService = gradeService;
    }

    [HttpPost]
    [Authorize(Policy = JwtExtensions.LecturerPolicy)]
    public async Task<IActionResult> GradeExam([FromBody] Request.GradeExamRequest request)
    {
        var result = await _gradeService.GradeExam(request);
        return Ok(result);
    }

    [HttpGet(template: "MyExamDetails")]
    [Authorize(Policy = JwtExtensions.StudentPolicy)]
    public async Task<IActionResult> GetMyExamDetails([FromQuery] Request.MyDetailsRequest request)
    {
        var result = _gradeService.MyExamDetails(request);
        return Ok(result);
    }
        
}