namespace SmartCenter.Service.Comment;

public class Request
{
    public class AddCommentRequest
    {
        public Guid LessonId { get; set; }
        public required string Content { get; set; }
        public Guid? ParentCommentId { get; set; }
    }

}