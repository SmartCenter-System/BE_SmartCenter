using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.EnrollmentService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public Service(AppDbContext dbContext,  IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<String> CreateEnrollment(Request.EnrollmentRequest request)// courseID //transactionID
    {
        var userId = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        var userIdGuid = Guid.Parse(userId);

        var student =  _dbContext.Students.Where(x => x.UserId == userIdGuid);

        var enrollment = new Enrollment();
        //học sinh đkí khóa học
        var Student = await student.FirstOrDefaultAsync();
        if (Student == null)
        {
            throw new Exception("Student not found");
        }
        //Kiểm tra coi thg này trả 1 phần hay trả hết
        var transaction = await _dbContext.Transactions.Where(x => x.Id == request.TransactionId).FirstOrDefaultAsync();
        if (transaction == null)
        {
            throw new Exception("Transaction not finish");
        }

        if (transaction.Status == "Part_Complete")
        {
            enrollment.Status = EnrollmentStatus.Deposited;
        }

        enrollment.Status = EnrollmentStatus.Paid;
        
        //các khóa học đã đkí
        var CourseEnrollment = await _dbContext.Enrollments.Where(x => x.StuId == Student.Id).ToListAsync();
        
        //Kiểm tra xem nó đã đki trước đây chưa
        bool isAlreadyEnrolled = CourseEnrollment.Any(x => x.CourseId == request.CourseId);
        if (isAlreadyEnrolled)
        {
            throw new Exception("Student has already registered for this course");
        }
        
        //kiểm tra xem khóa đó còn mở không
        var course = await _dbContext.Courses.Where(x => x.Id == request.CourseId).FirstOrDefaultAsync();
        if (course.IsActive == true)
        {
            if (course.MaxStudents != null && course.MaxStudents > 0)
            {
                //Số lượng đơn đkí của courseID
                var NumberEnrollmentCourse =await _dbContext.Enrollments.Where(x => x.CourseId == request.CourseId).CountAsync();
                //kiểm tra xem đã đầy chưa
                if (NumberEnrollmentCourse < course.MaxStudents)
                {
                    //tạo đơn đkí thôi
                    enrollment.StuId = Student.Id;
                    enrollment.CourseId = request.CourseId;
                    enrollment.TransactionId = request.TransactionId;
                    enrollment.EnrollmentDate = DateTimeOffset.UtcNow;
                    //thêm vào DB
                    _dbContext.Enrollments.Add(enrollment);
                    await _dbContext.SaveChangesAsync();
                    return "Enrolled Successfully";
                }
                throw new Exception("Course fully registered");
            }
        }
        throw new Exception("Course has closed");
    }

    public async Task<List<Response.MyEnrollmentResponse>> GetMyEnrollment()
    {
        var userId = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        var userIdGuid = Guid.Parse(userId);

        var student =  _dbContext.Students.Where(x => x.UserId == userIdGuid);

        var Student = await student.FirstOrDefaultAsync();
        if (Student == null)
        {
            throw new Exception("Student not found");
        }
        
        var enrollment = _dbContext.Enrollments.Where(x => x.StuId == Student.Id);

        var selectedEnrollment = enrollment.Select(x => new Response.MyEnrollmentResponse()
        {
            CourseName = x.Course.CourseName,
            BasePrice = x.Course.BasePrice,
            CourseType = x.Course.CourseType,
            ImgUrl =  x.Course.ImgUrl,
            IsActive =  x.Course.IsActive,
            StartAt =  x.Course.StartAt,
            EndAt =  x.Course.EndAt,
            AcademicYear = x.Course.AcademicYear,
            EnrollmentDate = x.EnrollmentDate,
            status = x.Status,
        });
        
        var result = await selectedEnrollment.ToListAsync();
        return result;
    }
}