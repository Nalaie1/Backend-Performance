namespace Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? AvatarUrl { get; set; }

    // Authentication
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    // Role-based access
    public string Role { get; set; } = "User"; // "Admin" hoặc "User"

    // Refresh token
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Relations
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}