using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.ExamManagementService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetStudentId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
                        .FindFirst("studentId")?.Value
                    ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin học sinh.");
        return Guid.Parse(claim);
    }

    private Guid GetLecturerId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
                        .FindFirst("lecturerId")?.Value
                    ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin giảng viên.");
        return Guid.Parse(claim);
    }

    private bool IsAdmin() =>
        _httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;


    public async Task<string> StartingExam(Guid ExamID)
    {
        //khi gọi API này nghĩa là bắt đầu làm bài
        var StudentId = GetStudentId();

        var Exam_Manament = new ExamManament()
        {
            ExamPaperId = ExamID,
            StudentId = StudentId,
            PointsOfStudent = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _dbContext.Add(Exam_Manament);
        await _dbContext.SaveChangesAsync();
        return "Starting Exam";
    }

    public async Task<string> SubmittedExam(Request.SubmitExamRequest request)
    {
        //ghi nhận câu trl của hc sinh
        var studentId = GetStudentId();

        var ExamManagement =
            await _dbContext.ExamManagements.FirstOrDefaultAsync(x =>
                x.StudentId == studentId && x.ExamPaperId == request.ExamId);

        if (ExamManagement == null)
        {
            return "Học sinh không có quyền hoặc phiên thi không tồn tại.";
        }

        var allExamDetails = await _dbContext.ExamPaperDetails
            .Include(x => x.Question)
            .ThenInclude(q => q.MultipleChoiceAnswers)
            .Where(x => x.ExamPaperId == request.ExamId)
            .ToListAsync();

        if (!allExamDetails.Any())
        {
            throw new Exception("Đề thi này chưa có câu hỏi nào được thiết lập.");
        }

        var answersToInsert = new List<ExamManementDetail>();
        decimal totalAutoGradedPoints = 0;
        var studentAnswers = request.ListAnswers ?? new List<Request.AnswerItems>();

        foreach (var detail in allExamDetails)
        {
            if (detail.Question == null) continue;

            bool isMultipleChoice = detail.Question.TypeOfQuestion == QuestionType.MultipleChoice;
            var studentAnswerForThisQuestion = studentAnswers.FirstOrDefault(x => x.QuestionId == detail.QuestionId);

            if (studentAnswerForThisQuestion == null)
            {
                answersToInsert.Add(new ExamManementDetail()
                {
                    ExamManementId = ExamManagement.Id,
                    ExamPaperDetailId = detail.Id,
                    IsMultiChoice = isMultipleChoice,
                    MultipleChoiceAnswerId = null,
                    Answer = null,
                    Point = 0,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
                continue;
            }

            if (isMultipleChoice)
            {
                var correctAnswersFromDB = detail.Question.MultipleChoiceAnswers
                    .Where(ans => ans.IsCorrect == true)
                    .Select(ans => ans.Id)
                    .ToList();

                var studentSelectedIds = studentAnswerForThisQuestion.MultipleChoiceAnswerIds ?? new List<Guid>();

                decimal calculatedPoint = 0;
                bool isFullyCorrect = (correctAnswersFromDB.Count == studentSelectedIds.Count) &&
                                      studentSelectedIds.All(id => correctAnswersFromDB.Contains(id));

                if (isFullyCorrect)
                {
                    calculatedPoint = detail.Question.Point;
                    totalAutoGradedPoints += calculatedPoint;
                }

                if (!studentSelectedIds.Any())
                {
                    answersToInsert.Add(new ExamManementDetail()
                    {
                        ExamManementId = ExamManagement.Id,
                        ExamPaperDetailId = detail.Id,
                        IsMultiChoice = true,
                        MultipleChoiceAnswerId = null,
                        Point = 0,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow
                    });
                }
                else
                {
                    bool isFirstRow = true;
                    foreach (var selectedId in studentSelectedIds)
                    {
                        answersToInsert.Add(new ExamManementDetail()
                        {
                            ExamManementId = ExamManagement.Id,
                            ExamPaperDetailId = detail.Id,
                            MultipleChoiceAnswerId = selectedId,
                            IsMultiChoice = true,
                            Point = isFirstRow ? calculatedPoint : 0,
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow
                        });
                        isFirstRow = false;
                    }
                }
            }
            else
            {
                answersToInsert.Add(new ExamManementDetail()
                {
                    ExamManementId = ExamManagement.Id,
                    ExamPaperDetailId = detail.Id,
                    MultipleChoiceAnswerId = null,
                    Answer = studentAnswerForThisQuestion.AnswerText,
                    IsMultiChoice = false,
                    Point = 0,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
            }
        }

        if (answersToInsert.Any())
        {
            await _dbContext.ExamManagementDetails.AddRangeAsync(answersToInsert);
        }

        ExamManagement.PointsOfStudent = totalAutoGradedPoints;
        _dbContext.ExamManagements.Update(ExamManagement);

        await _dbContext.SaveChangesAsync();

        return "Nộp bài thành công!";
    }

    public async Task<List<Response.ExamManagementResponse>> GetMyExams()
    {
        var studentId = GetStudentId();
        var Exam_Management = await _dbContext.ExamManagements
            .Where(x => x.StudentId == studentId)
            .Include(x => x.ExamPaper)
            .Select(x => new Response.ExamManagementResponse
            {
                PointOfStudent = x.PointsOfStudent,
                PointOfExam = x.ExamPaper.TotalPoints,
                Status = x.ExamPaper.Status,
                Title = x.ExamPaper.Title,
            })
            .ToListAsync();

        return Exam_Management;
    }

    public async Task<List<Response.ExamManagementResponse>> GetExamByExamsId(Guid ExamID)
    {
        var Exam_Management = _dbContext.ExamManagements
            .Where(x => x.ExamPaperId == ExamID)
            .Include(x => x.Student);
        var Exam_Paper = await _dbContext.ExamPapers.FirstOrDefaultAsync(x => x.Id == ExamID);
        var Selected = Exam_Management.Select(x => new Response.ExamManagementResponse
        {
            PointOfStudent = x.PointsOfStudent,
            PointOfExam = Exam_Paper.TotalPoints,
            Status = Exam_Paper.Status,
            Title = Exam_Paper.Title,
            StudentId = x.StudentId,
            FirstName = x.Student.User.FirstName,
            LastName = x.Student.User.LastName,
        });
        var result = await Selected.ToListAsync();
        return result;
    }
}