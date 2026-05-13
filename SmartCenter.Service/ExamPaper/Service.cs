using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.ExamPaper;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetLecturerId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
                        .FindFirst("lecturerId")?.Value
                    ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin giảng viên.");
        return Guid.Parse(claim);
    }

    private async Task AuthorizeExamAsync(Guid examId)
    {
        var lecturerId = GetLecturerId();
        var owns = await _dbContext.ExamPapers
            .AnyAsync(e => e.Id == examId && e.LecturerId == lecturerId);
        if (!owns)
            throw new UnauthorizedAccessException("Bạn không có quyền thao tác với đề thi này.");
    }

    public async Task<List<Response.ExamResponse>> GetExamsByCourseAsync(Guid courseId)
    {
        var lecturerId = GetLecturerId();
        var query = _dbContext.ExamPapers
            .Where(e => e.LecturerId == lecturerId && e.Lesson.CourseId == courseId)
            .Select(e => new Response.ExamResponse()
            {
                Id = e.Id,
                Title = e.Title,
                TotalPoints = e.TotalPoints,
                CountDown = e.CountDown,
                LessonId = e.LessonId,
                Status = e.Status,
                CreateAt = e.CreatedAt,
                Deadline = new Response.DeadlineResponse()
                {
                    Id = e.Deadline!.Id,
                    Title = e.Deadline.Title,
                    EndedAt = e.Deadline.EndedAt,
                    Status = e.Deadline.Status,
                }
            });
        var examPaper = await query.ToListAsync();
        return examPaper;
    }

    public async Task<Response.ExamResponse> CreateExamPaperAsync(Request.CreateExamPaperRequest request)
    {
        var lecturerId = GetLecturerId();

        var lesson = _dbContext.Lessons
            .FirstOrDefault(l => l.Id == request.LessonId && l.Section.Course.LecId == lecturerId);

        if (lesson == null)
            throw new Exception("Không tìm thấy bài học hoặc bạn không có quyền.");

        var exam = new Repository.Entity.ExamPaper()
        {
            Id = Guid.NewGuid(),
            LecturerId = lecturerId,
            LessonId = request.LessonId,
            Title = request.Title,
            CountDown = request.CountDown,
            TotalPoints = request.TotalPoints,
            Status = ExamPaperStatus.Open,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _dbContext.ExamPapers.Add(exam);

        var enrolledStudentIds = await _dbContext.Enrollments
            .Where(e => e.CourseId == lesson.CourseId && e.Status == EnrollmentStatus.Paid)
            .Select(e => e.Student.UserId)
            .ToListAsync();

        var notifications = enrolledStudentIds.Select(userId => new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Đề thi mới",
            Description = $"Đề thi \"{request.Title}\" vừa được tạo trong khóa học của bạn.",
            Type = "Email",
            RefId = exam.Id,
            RefType = "ExamPaper",
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await _dbContext.Notifications.AddRangeAsync(notifications);
        await _dbContext.SaveChangesAsync();

        var query = _dbContext.ExamPapers
            .Where(e => e.Id == exam.Id)
            .Select(e => new Response.ExamResponse()
            {
                Id = e.Id,
                Title = e.Title,
                TotalPoints = e.TotalPoints,
                CountDown = e.CountDown,
                LessonId = e.LessonId,
                Status = e.Status,
                CreateAt = e.CreatedAt,
                Deadline = new Response.DeadlineResponse()
                {
                    Id = e.Deadline!.Id,
                    Title = e.Deadline.Title,
                    EndedAt = e.Deadline.EndedAt,
                    Status = e.Deadline.Status,
                }
            });
        var examPaper = await query.FirstAsync();
        return examPaper;
    }


    public async Task<Response.ExamResponse> UpdateExamPaperAsync(Guid examId, Request.UpdateExamPaperRequest request)
    {
        await AuthorizeExamAsync(examId);

        var exam = await _dbContext.ExamPapers.FindAsync(examId);
        if (exam == null)
            throw new Exception("Không tìm thấy đề thi.");

        if (request.Title != null) exam.Title = request.Title;
        if (request.CountDown != null) exam.CountDown = request.CountDown.Value;
        if (request.TotalPoints != null) exam.TotalPoints = request.TotalPoints.Value;
        if (request.Status != null) exam.Status = request.Status.Value;

        await _dbContext.SaveChangesAsync();

        var query = _dbContext.ExamPapers
            .Where(e => e.Id == exam.Id)
            .Select(e => new Response.ExamResponse()
            {
                Id = e.Id,
                Title = e.Title,
                TotalPoints = e.TotalPoints,
                CountDown = e.CountDown,
                LessonId = e.LessonId,
                Status = e.Status,
                CreateAt = e.CreatedAt,
                Deadline = new Response.DeadlineResponse()
                {
                    Id = e.Deadline!.Id,
                    Title = e.Deadline.Title,
                    EndedAt = e.Deadline.EndedAt,
                    Status = e.Deadline.Status,
                }
            });
        var examPaperUpdated = await query.FirstAsync();
        return examPaperUpdated;
    }

    public async Task<Response.DeadlineResponse> SetDeadlineAsync(Guid examId, Request.SetDeadlineRequest request)
    {
        await AuthorizeExamAsync(examId);

        var exam = await _dbContext.ExamPapers
            .Include(e => e.Deadline)
            .FirstOrDefaultAsync(e => e.Id == examId);
        if (exam == null)
            throw new Exception("Không tìm thấy đề thi");

        if (exam.Deadline != null)
        {
            exam.Deadline.Title = request.Title;
            exam.Deadline.Status = DeadlineStatus.Processing;
            exam.Deadline.EndedAt = request.EndedAt;
        }
        else
        {
            var deadline = new Deadline
            {
                Id = Guid.NewGuid(),
                ExamPaperId = examId,
                Title = request.Title,
                EndedAt = request.EndedAt,
                Status = DeadlineStatus.Processing,
            };
            _dbContext.Deadlines.Add(deadline);
        }

        await _dbContext.SaveChangesAsync();

        var deadlineResponse = new Response.DeadlineResponse()
        {
            Id = exam.Deadline!.Id,
            Title = exam.Deadline.Title,
            EndedAt = exam.Deadline.EndedAt,
            Status = exam.Deadline.Status,
        };

        return deadlineResponse;
    }

    public async Task DeleteExamPaperAsync(Guid examId)
    {
        await AuthorizeExamAsync(examId);

        var exam = await _dbContext.ExamPapers.FindAsync(examId)
                   ?? throw new Exception("Không tìm thấy đề thi.");

        var hasSubmission = await _dbContext.ExamManagements
            .AnyAsync(e => e.ExamPaperId == examId);

        if (hasSubmission)
            throw new Exception("Không thể xóa đề thi đã có học sinh làm bài.");

        _dbContext.ExamPapers.Remove(exam);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Response.AddMultipleQuestionsResponse> AddMultipleQuestionsToExamAsync(Guid examId,
        Request.AddMultipleQuestionsRequest request)
    {
        if (request.Questions == null || !request.Questions.Any())
        {
            throw new ArgumentException("Danh sách câu hỏi không được để trống.");
        }

        await AuthorizeExamAsync(examId);

        var exam = await _dbContext.ExamPapers.FirstOrDefaultAsync(e => e.Id == examId);
        if (exam == null) throw new KeyNotFoundException("Không tìm thấy đề thi này trong hệ thống.");

        if (exam.Status == ExamPaperStatus.Closed || exam.Status == ExamPaperStatus.Deleted)
            throw new InvalidOperationException("Không thể thêm câu hỏi vào đề thi đã đóng hoặc đã bị xóa.");

        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var existingQuestionIds = await _dbContext.ExamPaperDetails
                .Where(ed => ed.Id == examId)
                .Select(ed => ed.QuestionId)
                .ToListAsync();

            int newTotalQuestionsCount = existingQuestionIds.Count + request.Questions.Count;
            decimal averagePoint = Math.Round(exam.TotalPoints / newTotalQuestionsCount, 2);

            if (existingQuestionIds.Any())
            {
                var existingQuestions = await _dbContext.Questions
                    .Where(q => existingQuestionIds.Contains(q.Id))
                    .ToListAsync();

                foreach (var q in existingQuestions)
                {
                    q.Point = averagePoint;
                }

                _dbContext.Questions.UpdateRange(existingQuestions);
            }

            var questionsToAdd = new List<Question>();
            var examDetailsToAdd = new List<ExamPaperDetail>();
            var mcAnswersToAdd = new List<MultipleChoiceAnswer>();
            var essayAnswersToAdd = new List<EssayAnswer>();

            var responseItems = new List<Response.QuestionDetailResponse>();

            for (int i = 0; i < request.Questions.Count; i++)
            {
                var qRequest = request.Questions[i];
                var questionId = Guid.NewGuid();

                var newQuestion = new Question
                {
                    Id = questionId,
                    Title = qRequest.Title,
                    TypeOfQuestion = qRequest.TypeOfQuestion,
                    Point = averagePoint
                };
                questionsToAdd.Add(newQuestion);

                var detailResponse = new Response.QuestionDetailResponse
                {
                    QuestionId = questionId,
                    Title = newQuestion.Title,
                    TypeOfQuestion = newQuestion.TypeOfQuestion
                };

                if (qRequest.TypeOfQuestion == QuestionType.MultipleChoice)
                {
                    if (qRequest.MultipleChoiceAnswers == null || !qRequest.MultipleChoiceAnswers.Any())
                        throw new ArgumentException(
                            $"Lỗi ở câu hỏi số {i + 1}: Câu hỏi trắc nghiệm phải có ít nhất 1 đáp án.");

                    if (!qRequest.MultipleChoiceAnswers.Any(a => a.IsCorrect))
                        throw new ArgumentException($"Lỗi ở câu hỏi số {i + 1}: Phải có ít nhất 1 đáp án đúng.");

                    var mcAnswers = qRequest.MultipleChoiceAnswers.Select(a => new MultipleChoiceAnswer()
                    {
                        Id = Guid.NewGuid(),
                        QuestionId = questionId,
                        Content = a.Content,
                        IsCorrect = a.IsCorrect
                    }).ToList();

                    mcAnswersToAdd.AddRange(mcAnswers);

                    detailResponse.MultipleChoiceAnswers = mcAnswers.Select(a => new Response.AnswerOptionResponse
                    {
                        AnswerId = a.Id,
                        Content = a.Content,
                        IsCorrect = a.IsCorrect
                    }).ToList();
                }
                else if (qRequest.TypeOfQuestion == QuestionType.Essay)
                {
                    if (string.IsNullOrWhiteSpace(qRequest.EssayContext))
                        throw new ArgumentException(
                            $"Lỗi ở câu hỏi số {i + 1}: Vui lòng cung cấp nội dung đề bài (EssayContext).");

                    var essayAnswer = new EssayAnswer()
                    {
                        Id = Guid.NewGuid(),
                        QuestionId = questionId,
                        Content = qRequest.EssayContext
                    };
                    essayAnswersToAdd.Add(essayAnswer);

                    detailResponse.EssayContext = essayAnswer.Content;
                }

                examDetailsToAdd.Add(new ExamPaperDetail
                {
                    Id = Guid.NewGuid(),
                    ExamPaperId = examId,
                    QuestionId = questionId
                });

                responseItems.Add(detailResponse);
            }

            await _dbContext.Questions.AddRangeAsync(questionsToAdd);
            await _dbContext.ExamPaperDetails.AddRangeAsync(examDetailsToAdd);

            if (mcAnswersToAdd.Any()) await _dbContext.MultipleChoiceAnswers.AddRangeAsync(mcAnswersToAdd);
            if (essayAnswersToAdd.Any()) await _dbContext.EssayAnswers.AddRangeAsync(essayAnswersToAdd);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return new Response.AddMultipleQuestionsResponse
            {
                AddedQuestions = responseItems,
                NewAveragePoint = averagePoint
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Response.ExamDetailResponse> GetExamDetailAsync(Guid examId)
    {
        var examDetail = await _dbContext.ExamPapers
            .Where(e => e.Id == examId)
            .Select(e => new Response.ExamDetailResponse
            {
                Id = e.Id,
                Title = e.Title,
                CountDown = e.CountDown,
                TotalPoints = e.TotalPoints,
                Status = e.Status,
                LessonId = e.LessonId,
                ListQuestions = e.ExamPaperDetails.Select(epd => new Response.QuestionDetailResponse
                {
                    QuestionId = epd.Question.Id,
                    Title = epd.Question.Title,
                    TypeOfQuestion = epd.Question.TypeOfQuestion,
                    MultipleChoiceAnswers =
                        epd.Question.TypeOfQuestion == QuestionType.MultipleChoice
                            ? epd.Question.MultipleChoiceAnswers.Select(mc => new Response.AnswerOptionResponse
                            {
                                AnswerId = mc.Id,
                                Content = mc.Content,
                                IsCorrect = mc.IsCorrect
                            }).ToList()
                            : new List<Response.AnswerOptionResponse>(),
                    EssayContext = epd.Question.TypeOfQuestion == QuestionType.Essay
                        ? epd.Question.EssayAnswers.Select(ea => ea.Content).FirstOrDefault()
                        : null
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (examDetail == null)
        {
            throw new Exception("Không tìm thấy bài kiểm tra");
        }

        return examDetail;
    }
}