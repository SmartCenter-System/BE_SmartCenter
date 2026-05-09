namespace SmartCenter.Service.Comment;

public class Response
{
    public class CommentResponse
    {
        public Guid Id { get; set; }
        public Guid LessonId { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Guid? ParentCommentId { get; set; }
        public int DepthLevel { get; set; }
        public bool IsLocked { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<CommentResponse> Replies { get; set; } = new();
    }
}