namespace Application.DTOs.Comment;

/// <summary>
///     Biểu diễn một comment ở dạng phẳng, có kèm depth và path để render/đếm thứ tự.
/// </summary>
public class CommentFlattenDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public Guid PostId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public int Depth { get; set; } // Level trong tree
    public string Path { get; set; } = null!; // e.g., "1.2.3" để hiển thị hierarchy
}