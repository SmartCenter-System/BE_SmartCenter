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
        var userId = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        var userIdGuid = Guid.Parse(userId);
        var student = _dbContext.Students.FirstOrDefault(x => x.Id == userIdGuid);
        return student.Id;
    }

    public async Task<string> StartingExam(Guid ExamID)
    {
        //khi gọi API này nghĩa là bắt đầu làm bài
        var StudentId = GetStudentId();
        var Exam_Manament = new ExamManament()
        {
            ExamPaperId = ExamID,
            StudentId = StudentId,
            PointsOfStudent = 0,
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

        // Lấy danh sách Question đã trả lời
        var submittedQuestionIds = request.ListAnswers.Select(x => x.QuestionId).ToList();

        //Gộp toàn bộ câu hỏi và đáp án
        var validExamDetails = await _dbContext.ExamPaperDetails
            .Include(x => x.Question)
            .ThenInclude(q => q.MultipleChoiceAnswers)
            .Where(x => x.ExamPaperId == request.ExamId && submittedQuestionIds.Contains(x.QuestionId))
            .ToListAsync();

        var answersToInsert = new List<ExamManementDetail>();
        int totalAutoGradedPoints = 0;

        foreach (var answerReq in request.ListAnswers)
        {
            var matchedDetail = validExamDetails.FirstOrDefault(x => x.QuestionId == answerReq.QuestionId);

            // Nếu học sinh gửi ID câu hỏi không có trong đề, bỏ qua ngay lập tức
            if (matchedDetail == null || matchedDetail.Question == null)
                continue;

            bool isMultipleChoiceDB = matchedDetail.Question.TypeOfQuestion == QuestionType.MultipleChoice;

            if (isMultipleChoiceDB)
            {
                // Lấy ID đáp án đúng
                var correctAnswersFromDB = matchedDetail.Question.MultipleChoiceAnswers
                    .Where(ans => ans.IsCorrect == true)
                    .Select(ans => ans.Id)
                    .ToList();

                // Danh sách ID đáp án học sinh chọn
                var studentSelectedIds = answerReq.MultipleChoiceAnswerIds ?? new List<Guid>();

                //Chấm điểm phải chọn đủ và chọn đúng đáp án mới cho điểm
                int calculatedPoint = 0;
                bool isFullyCorrect = (correctAnswersFromDB.Count == studentSelectedIds.Count) &&
                                      studentSelectedIds.All(id => correctAnswersFromDB.Contains(id));

                if (isFullyCorrect)
                {
                    calculatedPoint = matchedDetail.Question.Point;
                    totalAutoGradedPoints += calculatedPoint;
                }

                //Nếu học sinh không chọn đáp án nào
                if (!studentSelectedIds.Any())
                {
                    answersToInsert.Add(new ExamManementDetail()
                    {
                        ExamManementId = ExamManagement.Id,
                        MultipleChoiceAnswerId = null,
                        ExamPaperDetailId = matchedDetail.Id,
                        IsMultiChoice = true,
                        Point = 0
                    });
                    continue;
                }

                //Tạo nhiều dòng cho nhiều đáp án và xử lý chống nhân đôi điểm
                bool isFirstRow = true;
                foreach (var selectedId in studentSelectedIds)
                {
                    answersToInsert.Add(new ExamManementDetail()
                    {
                        ExamManementId = ExamManagement.Id,
                        ExamPaperDetailId = matchedDetail.Id,
                        MultipleChoiceAnswerId = selectedId,
                        IsMultiChoice = true,
                        Point = isFirstRow ? calculatedPoint : 0
                    });
                    isFirstRow = false;
                }
            }
            // Câu tự luận
            else
            {
                answersToInsert.Add(new ExamManementDetail()
                {
                    ExamManementId = ExamManagement.Id,
                    ExamPaperDetailId = matchedDetail.Id,
                    MultipleChoiceAnswerId = null,
                    Answer = answerReq.AnswerText,
                    IsMultiChoice = false,
                    Point = null // Treo điểm chờ giáo viên chấm
                });
            }
        }


        if (answersToInsert.Any())
        {
            await _dbContext.ExamManagementDetails.AddRangeAsync(answersToInsert);

            // Cập nhật tổng điểm trắc nghiệm vào phiên thi hiện tại
            ExamManagement.PointsOfStudent = totalAutoGradedPoints;
            _dbContext.ExamManagements.Update(ExamManagement);

            await _dbContext.SaveChangesAsync();
        }

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
        
        return  Exam_Management;
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

