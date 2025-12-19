namespace Application.DTOs.Post;

public class PostCreateDto
{
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public Guid UserId { get; set; }
}