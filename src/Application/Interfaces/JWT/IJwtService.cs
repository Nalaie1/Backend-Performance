using Domain.Entities;

namespace Domain.Interfaces;

public interface IJwtService
{
    // Tạo JWT token cho user
    String GenerateAccessToken(User user);
    
    // Tạo refresh token
    String GenerateRefreshToken(); 
}