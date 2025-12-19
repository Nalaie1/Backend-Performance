using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    // Phương thức lấy người dùng theo username
    public Task<User?> GetByUsernameAsync(string username)
    {
        return _context.Users.FirstOrDefaultAsync(x => x.Username == username);
    }

    // Phương thức lấy người dùng theo id
    public Task<User?> GetByIdAsync(Guid id)
    {
        return _context.Users.FirstOrDefaultAsync(x => x.Id == id);
    }

    // Phương thức lấy người dùng theo refresh token
    public Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return _context.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
    }

    // Phương thức cập nhật refresh token và thời gian hết hạn
    public async Task UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiry)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = expiry;
        await _context.SaveChangesAsync();
    }

    // Phương thức thu hồi refresh token
    public async Task<bool> RevokeRefreshTokenAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _context.SaveChangesAsync();
        return true;
    }
}