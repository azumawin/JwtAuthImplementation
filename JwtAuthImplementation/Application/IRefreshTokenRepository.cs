namespace JwtAuthImplementation.Application;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetRefreshTokenByHashAsync(byte[] hash, CancellationToken ct = default);

    Task ReplaceRefreshTokenAsync(
        long userId,
        byte[] hash,
        DateTimeOffset expiresAt,
        CancellationToken ct = default
    );

    Task RevokeRefreshTokenAsync(long userId, CancellationToken ct = default);
}
