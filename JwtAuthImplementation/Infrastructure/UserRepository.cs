using JwtAuthImplementation.Domain;
using JwtAuthImplementation.Shared;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using User = JwtAuthImplementation.Domain.User;
// needed because they share the same name, but one is scaffolded from db and one is a domain model.
// the only place where both are imported is a repository implementation.
using UserRow = JwtAuthImplementation.Infrastructure.User;

namespace JwtAuthImplementation.Infrastructure;

// i need to somehow seperate Domain.User and Infrastructure.User and add an error type for returning
public class UserRepository(AppDbContext _db) : IUserRepository
{
    // i dont really like assuming from caller pov that just because an add user operation failed that a unique constraint was violated
    // i could use an enum AddUserResult but i only have 2 cases right now: success or failure, so i could use bool, but i still prefer
    // the return type to carry the reason why the add failed, explicitly.
    public async Task<Result<User, AddError>> AddAsync(
        string username,
        string passwordHash,
        CancellationToken ct = default
    )
    {
        try
        {
            UserRow user = new() { Username = username, PasswordHash = passwordHash };
            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pgEx
                && pgEx.SqlState == PostgresErrorCodes.UniqueViolation
            )
        {
            return UserRepositoryAddResult.UsernameConflict;
        }
        return UserRepositoryAddResult.Success;
    }

    // looks a bit weird bcus i could just call this, but i guess this wrapper makes me not couple to the AppDbContext and hides this behind an interface
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        User? user = await _db.Users.SingleOrDefaultAsync(u => u.Username == username, ct);
        return user;
    }
}
