using MailKit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Service.MailService;

namespace SmartCenter.Service.GradeService;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly MailService.IService _mailService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext dbContext, MailService.IService mailService, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _mailService = mailService;
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
    
    private async Task AuthorizeLectureAsync(Guid examId)
    {
        //phải là giáo viên tạo bài kiểm tra này thì mới được chấm
        if (IsAdmin()) return;
        var lecturerId = GetLecturerId();
        var owns = await _dbContext.ExamPapers
            .AnyAsync(s => s.Id == examId && s.LecturerId == lecturerId);
        if (!owns)
            throw new UnauthorizedAccessException("Bạn không có quyền thao tác với bài này.");
    }

    private async Task AuthorizeStudentAsync(Guid examId)
    {
        //phải là học sinh đã làm bài kiểm tra đó thì mới xem được chi tiết bài làm
        if (IsAdmin()) return;
        var studentId = GetStudentId();
        var owns = await _dbContext.ExamManagements
            .AnyAsync(s => s.ExamPaperId == examId && s.StudentId == studentId);
        if (!owns)
            throw new UnauthorizedAccessException("Bạn không có quyền thao tác với bài này.");
    }

    public async Task<string> GradeExam(Request.GradeExamRequest request)
    {
        await AuthorizeLectureAsync(request.ExamId);
        
        var User = await _dbContext.Users.Where(x => x.Student.Id == request.StudentId).FirstOrDefaultAsync();

        var ExamPaper = await _dbContext.ExamPapers.Where(x => x.Id == request.ExamId).FirstOrDefaultAsync();
            
        var examManagement = await _dbContext.ExamManagements
            .Include(em => em.ExamManementDetails)
            .ThenInclude(emd => emd.ExamPaperDetail)
            .ThenInclude(epd => epd.Question)
            .FirstOrDefaultAsync(em => em.ExamPaperId == request.ExamId && em.StudentId == request.StudentId);

        if (examManagement == null)
        {
            throw new Exception("Không tìm thấy dữ liệu bài thi của học sinh này.");
        }

        // Duyệt qua danh sách điểm và feedback giáo viên gửi 
        foreach (var gradeRequest in request.GradeDetails)
        {
            // Tìm câu hỏi
            var detailToUpdate = examManagement.ExamManementDetails
                .FirstOrDefault(d => d.ExamPaperDetailId == gradeRequest.ExamManagementDetailId);

            if (detailToUpdate == null) continue;

            // FeedBack
            if (!string.IsNullOrEmpty(gradeRequest.Feedback))
            {
                detailToUpdate.Feedback = gradeRequest.Feedback;
            }

            if (detailToUpdate.IsMultiChoice)
            {
                continue;
            }
            else
            {
                if (gradeRequest.Point.HasValue)
                {
                    var maxPoint = detailToUpdate.ExamPaperDetail.Question.Point;

                    if (gradeRequest.Point.Value > maxPoint)
                    {
                        throw new Exception(
                            $"Lỗi ở câu hỏi '{detailToUpdate.ExamPaperDetail.Question.Title}': Điểm tự luận ({gradeRequest.Point.Value}) không được vượt quá điểm tối đa ({maxPoint}).");
                    }

                    // Cập nhật điểm 
                    detailToUpdate.Point = gradeRequest.Point.Value;
                }
                else
                {
                    detailToUpdate.Point = 0;
                }
            }
        }

        examManagement.PointsOfStudent = examManagement.ExamManementDetails.Sum(d => d.Point ?? 0);

        var check = await _dbContext.SaveChangesAsync();
        if (check > 0)
        {
            await _mailService.SendMail(new MailContent()
            {
                To = User.Email,
                Subject = "SmartCenter - Grade Exam Finished",
                Body = BuildGradeNotificationEmailBody($"{User.FirstName} {User.LastName}", $"{ExamPaper.Title}",
                    examManagement.PointsOfStudent)
            });
        }

        return "Cập nhật điểm và phản hồi thành công!";
    }


    public async Task<Response.MyExamDetailsResponse> MyExamDetails(Request.MyDetailsRequest request)
    {
        await AuthorizeStudentAsync(request.ExamId);
        
        var studentId = GetStudentId();
        
        var examSubmission = await _dbContext.ExamManagements
            .Include(em => em.ExamPaper)
            .Include(em => em.ExamManementDetails)
            .ThenInclude(emd => emd.ExamPaperDetail)
            .ThenInclude(epd => epd.Question)
            .ThenInclude(q => q.MultipleChoiceAnswers)
            .FirstOrDefaultAsync(em => em.ExamPaperId == request.ExamId && em.StudentId == studentId);
        
        if (examSubmission == null)
        {
            throw new Exception("Không tìm thấy kết quả bài thi của học sinh này hoặc học sinh chưa làm bài.");
        }
        
        var response = new Response.MyExamDetailsResponse
        {
            ExamId = examSubmission.ExamPaperId,
            ExamTitle = examSubmission.ExamPaper?.Title ?? "Bài thi không xác định",
            TotalScore = examSubmission.PointsOfStudent,

            QuestionDetails = examSubmission.ExamManementDetails.Select(detail =>
            {
                var question = detail.ExamPaperDetail?.Question;
                string studentAnswerStr = string.Empty;
                bool? isCorrectAnswer = null;

                // phân loại câu hỏi
                if (detail.IsMultiChoice)
                {
                    // Lấy nội dung của đáp án trắc nghiệm mà học sinh đã chọn
                    var selectedMc = question?.MultipleChoiceAnswers
                        .FirstOrDefault(mc => mc.Id == detail.MultipleChoiceAnswerId);
                    studentAnswerStr = selectedMc?.Content ?? "Chưa chọn đáp án";
                    isCorrectAnswer = selectedMc?.IsCorrect ?? false;
                }
                else
                {
                    // Xử lý cho câu tự luận
                    studentAnswerStr = detail.Answer ?? "";
                    isCorrectAnswer = null;
                }

                return new Response.ExamQuestionDetail
                {
                    QuestionId = question?.Id ?? Guid.Empty,
                    Title = question?.Title ?? "",
                    Context = "", 
                    IsCorrect = isCorrectAnswer,
                    MultipleChoiceAnswerId = detail.MultipleChoiceAnswerId,
                    StudentAnswerText = studentAnswerStr,
                    Points = detail.Point ?? 0,
                    Feedback = detail.Feedback ?? ""
                };
            }).ToList()
        };

        return response;
    }

    private static string BuildGradeNotificationEmailBody(string fullName, string examTitle, decimal totalScore) => $"""
         <!DOCTYPE html>
         <html lang="vi">
         <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
         <body style='font-family:Arial,sans-serif;background:#f4f6f8;margin:0;padding:0;'>
             <table width='100%' cellpadding='0' cellspacing='0'>
                 <tr>
                     <td align='center' style='padding:40px 0;'>
                         <table width='600' cellpadding='0' cellspacing='0'
                                style='background:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08);'>

                             <!-- Header -->
                             <tr>
                                 <td style='background:#4F46E5;padding:30px;text-align:center;'>
                                     <h1 style='color:#ffffff;margin:0;font-size:24px;'>SmartCenter</h1>
                                 </td>
                             </tr>

                             <!-- Body -->
                             <tr>
                                 <td style='padding:40px 30px;color:#333333;'>
                                     <p style='font-size:18px;'>Xin chào <strong>{fullName}</strong>,</p>
                                     <p>Bài thi <strong>{examTitle}</strong> của bạn đã được giáo viên chấm điểm hoàn tất.</p>
                                     <p>Tổng điểm của bạn là:</p>

                                     <div style='text-align:center;margin:36px 0;'>
                                         <span style='background:#f0fdf4;border:2px solid #10B981;padding:16px 40px;
                                                      border-radius:8px;font-size:32px;font-weight:bold;
                                                      color:#10B981;'>
                                             {totalScore:0.##} / 100
                                         </span>
                                     </div>

                                     <p style='color:#555;font-size:15px;line-height:1.6;'>
                                         Bạn có thể đăng nhập vào hệ thống SmartCenter để xem chi tiết điểm từng câu hỏi (bao gồm trắc nghiệm và tự luận) cũng như đọc các nhận xét (feedback) từ giáo viên.
                                     </p>
                                     <p style='color:#888;font-size:13px;margin-top:24px;border-top:1px solid #eee;padding-top:16px;'>
                                         Đây là email thông báo tự động từ hệ thống, vui lòng không trả lời email này.
                                     </p>
                                 </td>
                             </tr>

                             <!-- Footer -->
                             <tr>
                                 <td style='background:#f4f6f8;padding:20px;text-align:center;font-size:12px;color:#888;'>
                                     &copy; 2026 SmartCenter. All rights reserved.
                                 </td>
                             </tr>
                         </table>
                     </td>
                 </tr>
             </table>
         </body>
         </html>
         """;
}