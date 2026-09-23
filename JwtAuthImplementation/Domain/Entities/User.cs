namespace JwtAuthImplementation.Domain.Entities;

public class User
{
    public int Id { get; }
    public string Username { get; }
    public string PasswordHash { get; }
    public DateTime CreatedAt { get; }

    public User(int id, string username, string passwordHash, DateTime createdAt)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "Id cannot be negative");
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        if (createdAt == default)
            throw new ArgumentException("CreatedAt must be set", nameof(createdAt));
        if (createdAt > DateTime.UtcNow)
            throw new ArgumentException("CreatedAt cannot be in the future", nameof(createdAt));

        Id = id;
        Username = username;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
    }
}
