namespace Domain.Entities;

public class Post
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string? ImageUrl { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}