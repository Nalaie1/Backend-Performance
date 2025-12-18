namespace Domain.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public string? AttachmentUrl { get; set; }

    // Recursive
    public Guid? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }

    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}