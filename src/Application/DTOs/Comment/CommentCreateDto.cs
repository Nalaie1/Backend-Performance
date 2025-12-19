namespace Application.DTOs.Comment;

/// <summary>
///     Dữ liệu đầu vào để tạo bình luận mới
/// </summary>
public class CommentCreateDto
{
    public string Content { get; set; } = null!;
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public Guid? ParentCommentId { get; set; }
}