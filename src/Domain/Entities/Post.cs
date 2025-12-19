namespace Domain.Entities;

public class Post
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public Guid AuthorId { get; set; }  // <-- ai tạo bài viết
    public User User { get; set; } = null!; // navigation property
    
    public string? ImageUrl { get; set; }

    public List<Comment> Comments { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}