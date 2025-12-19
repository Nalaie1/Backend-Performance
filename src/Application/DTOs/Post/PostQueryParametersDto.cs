namespace Application.DTOs.Post;

public class PostQueryParametersDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc"; // "asc" or "desc"
    public string? SearchTerm { get; set; }
    public Guid? UserId { get; set; }
    public Guid? PostId { get; set; }
}