namespace Application.DTOs.Comment;

/// <summary>
///     Tham số query cho comment (khi cần mở rộng search/sort/paging ở comment).
/// </summary>
public class CommentQueryParametersDto
{
    public Guid? PostId { get; set; }
    public Guid? UserId { get; set; }
    public string? SearchTerm { get; set; } // Filter theo content
    public string? SortBy { get; set; } // cột để sort
    public string? SortDirection { get; set; } // "asc" / "desc"
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool IncludeReplies { get; set; } = true;
}