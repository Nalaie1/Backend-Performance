using Application.DTOs.User;

namespace Application.Interfaces.Auth;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(string username, string password);
    Task<LoginResponseDto?> RefreshAsync(string refreshToken);
    Task LogoutAsync(Guid userId);
}