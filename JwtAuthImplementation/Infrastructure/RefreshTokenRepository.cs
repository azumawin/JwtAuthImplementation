using JwtAuthImplementation.Domain;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthImplementation.Infrastructure;

public class RefreshTokenRepository(AppDbContext _db) : IRefreshTokenRepository
{
    public async Task RevokeRefreshTokenAsync(long userId, CancellationToken ct = default)
    {
        await _db.RefreshTokens.Where(r => r.UserId == userId).ExecuteDeleteAsync(ct);
    }

    public async Task ReplaceRefreshTokenAsync(
        long userId,
        byte[] refreshTokenHash,
        DateTimeOffset refreshTokenExpiresAt,
        CancellationToken ct = default
    )
    {
        await _db.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO refresh_tokens(token_hash, expires_at, user_id)
            VALUES ({refreshTokenHash}, {refreshTokenExpiresAt}, {userId})
            ON CONFLICT (user_id) DO UPDATE SET
                token_hash = EXCLUDED.token_hash,
                expires_at = EXCLUDED.expires_at,
                created_at = now()
            """,
            ct
        );
    }

    public async Task<RefreshToken?> GetRefreshTokenByHashAsync(
        byte[] hash,
        CancellationToken ct = default
    )
    {
        RefreshToken? rt = await _db.RefreshTokens.FindAsync(hash, ct);
        return rt;
    }
}
