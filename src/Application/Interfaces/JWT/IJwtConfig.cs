namespace Domain.Interfaces;

public interface IJwtConfig
{
    int AccessTokenMinutes { get; }
    int RefreshTokenDays { get; }
}