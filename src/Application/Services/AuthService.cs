using System.Security.Cryptography;
using System.Text;
using Application.DTOs.User;
using Domain.Interfaces;


namespace Application.Services;

public class AuthService
{
    private readonly IUserRepository _users;
    private readonly IJwtService _jwt;
    private readonly IJwtConfig _jwtConfig;


    public AuthService(
        IUserRepository users,
        IJwtService jwt,
        IJwtConfig jwtConfig)
    {
        _users = users;
        _jwt = jwt;
        _jwtConfig = jwtConfig;
    }
    
    // ================= LOGIN =====================
    public async Task<LoginResponseDto?> LoginAsync(string username, string password)
    {
        var user = await _users.GetByUsernameAsync(username);
        if (user == null || !VerifyPassword(password, user.PasswordHash))
            return null;

        var access = _jwt.GenerateAccessToken(user);
        var refresh = _jwt.GenerateRefreshToken();

        await _users.UpdateRefreshTokenAsync(
            user.Id,
            refresh,
            DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenDays)
        );
        return new LoginResponseDto
        {
            AccessToken = access,
            RefreshToken = refresh,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtConfig.AccessTokenMinutes)
        };
    }

    // ================= REFRESH TOKEN =====================
    public async Task<LoginResponseDto?> RefreshAsync(string refreshToken)
    {
        // Kiểm tra refresh token có tồn tại không
        var user = await _users.GetByRefreshTokenAsync(refreshToken);

        // Refresh token invalid hoặc user không tồn tại
        if (user == null)
            return null;

        // Refresh token đã hết hạn → bắt login lại
        if (user.RefreshTokenExpiryTime == null ||
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return null;

        // Refresh token hợp lệ => cấp token mới
        var newAccessToken = _jwt.GenerateAccessToken(user);
        var newRefreshToken = _jwt.GenerateRefreshToken();

        // Lưu refresh token mới vào DB
        await _users.UpdateRefreshTokenAsync(
            user.Id,
            newRefreshToken,
            DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenDays)
        );

        // Trả về DTO mới
        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtConfig.AccessTokenMinutes)
        };
    }

    // ================= LOGOUT =====================
    public async Task LogoutAsync(Guid userId)
    {
        await _users.RevokeRefreshTokenAsync(userId);
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hashString = Convert.ToHexString(hashBytes);
        return hashString.Equals(storedHash, StringComparison.OrdinalIgnoreCase);
    }
}