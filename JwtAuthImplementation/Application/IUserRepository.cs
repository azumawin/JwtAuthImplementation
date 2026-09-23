using JwtAuthImplementation.Domain;

namespace JwtAuthImplementation.Application;

public interface IUserRepository
{
    Task<UserRepositoryAddResult> AddAsync(
        string username,
        string passwordHash,
        CancellationToken ct = default
    );
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
}

public enum UserRepositoryAddResult
{
    Success,
    UsernameConflict,
};
