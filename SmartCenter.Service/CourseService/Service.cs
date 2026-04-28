using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;

namespace SmartCenter.Service.Course;

public class Service : IService
{
    private readonly AppDbContext _Dbcontext;
    public Service(AppDbContext dbContext)
    {
        _Dbcontext = dbContext;
    }
    public async Task<List<Response.CourseResponse>> GetAllCourses(string? search)
    {
        var query = _Dbcontext.Courses.Where(x => true);
        if (search != null)
        {
            query = query.Where(x => x.CourseName.Contains(search));
        }

        query = query.OrderByDescending(x => x.AcademicYear);

        var querySelected = query.Select(x => new Response.CourseResponse()
        {
            LecturerId = x.LecId,
            CourseName =  x.CourseName,
            Description =  x.Description,
            BasePrice =  x.BasePrice,
            CourseType = x.CourseType,
            ImgUrl = x.ImgUrl,
            StartAt =  x.StartAt,
            EndAT =   x.EndAt,
            Max_Students = x.MaxStudents,
            AcademicYear =  x.AcademicYear,
        });
        
        var listResult = await querySelected.ToListAsync();
        
        return listResult;
    }

    public async Task<Response.CourseResponse> GetCourseById(Guid id)
    {
        var query = _Dbcontext.Courses.Where(x => x.Id == id);
        
        var querySelected = query.Select(x => new Response.CourseResponse()
        {
            LecturerId = x.LecId,
            CourseName =  x.CourseName,
            Description =  x.Description,
            BasePrice =  x.BasePrice,
            CourseType = x.CourseType,
            ImgUrl = x.ImgUrl,
            StartAt =  x.StartAt,
            EndAT =   x.EndAt,
            Max_Students = x.MaxStudents,
            AcademicYear =  x.AcademicYear,
        });
        
        var result = await querySelected.FirstOrDefaultAsync();
        return result;
    }
}