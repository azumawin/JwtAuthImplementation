namespace JwtAuthImplementation.Domain.Entities;

public class RefreshToken
{
    public byte[] TokenHash { get; }
    public DateTime ExpiresAt { get; }
    public int UserId { get; }
    public DateTime CreatedAt { get; }

    public RefreshToken(byte[] tokenHash, DateTime expiresAt, int userId, DateTime createdAt)
    {
        ArgumentNullException.ThrowIfNull(tokenHash);
        if (tokenHash.Length == 0)
            throw new ArgumentException("TokenHash cannot be empty", nameof(tokenHash));
        ArgumentOutOfRangeException.ThrowIfNegative(userId);
        if (createdAt == default)
            throw new ArgumentException("CreatedAt must be set", nameof(createdAt));
        if (createdAt > DateTime.UtcNow)
            throw new ArgumentException("CreatedAt cannot be in the future", nameof(createdAt));
        if (expiresAt <= createdAt)
            throw new ArgumentException("ExpiresAt must be after CreatedAt", nameof(expiresAt));

        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        UserId = userId;
        CreatedAt = createdAt;
    }
}
