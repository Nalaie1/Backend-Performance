using Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class JwtConfig : IJwtConfig
{
    public int AccessTokenMinutes { get; }
    public int RefreshTokenDays { get; }

    public JwtConfig(IConfiguration config)
    {
        AccessTokenMinutes =
            int.Parse(config["JwtSettings:AccessTokenMinutes"]!);

        RefreshTokenDays =
            int.Parse(config["JwtSettings:RefreshTokenDays"]!);
    }
}