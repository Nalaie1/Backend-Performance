namespace Application.DTOs.Comment;

/// <summary>
///     Dữ liệu đầu vào để cập nhật bình luận
/// </summary>
public class CommentUpdateDto
{
    public string Content { get; set; } = null!;
}