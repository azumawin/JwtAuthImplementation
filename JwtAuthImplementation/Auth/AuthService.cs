/*
 * this service is supposed to be called by the controller only, so it is assumed that the data is already validated by fluentvalidation
 */
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using JwtAuthImplementation.Auth.Dtos;
using JwtAuthImplementation.Auth.JwtHandling;
using JwtAuthImplementation.Data;
using JwtAuthImplementation.Entities;
using JwtAuthImplementation.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace JwtAuthImplementation.Auth;

public class AuthService(AppDbContext _db, IOptions<AuthConfig> options, JwtHandler _jwtHandler)
{
    private static readonly string _dummyHash = BCrypt.Net.BCrypt.HashPassword(
        PreHash("DummyHashToPreventTimingAttacks")
    );
    private readonly AuthConfig _config = options.Value;

    /// <summary>
    /// Registers a new user into the database.
    /// </summary>
    /// <remarks>
    /// Assumes username and password are already validated by the FluentValidation pipeline.
    /// </remarks>
    /// <returns>
    /// A Result containing the created <see cref="UserSummary"/> on success, or a
    /// <see cref="RegisterError"/> describing the failure.
    /// </returns>
    public async Task<Result<UserSummary, RegisterError>> RegisterAsync(
        string username,
        string password
    )
    {
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(PreHash(password));
        User user = new() { Username = username, PasswordHash = passwordHash };

        // coupled to postgres but dont plan on changing so idrc
        try
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pgEx
                && pgEx.SqlState == PostgresErrorCodes.UniqueViolation
            )
        {
            return RegisterError.UsernameTaken;
        }

        UserSummary resp = new(user.Id, user.Username, user.CreatedAt);
        return resp;
    }

    /// <summary>
    /// Logs in a user by verifying credentials and issuing fresh JWT and refresh tokens.
    /// </summary>
    /// <remarks>
    /// Assumes username and password are already validated by the FluentValidation pipeline.
    /// </remarks>
    /// <returns>
    /// A Result containing the created <see cref="AuthResponse"/> on success, or a
    /// <see cref="LoginError"/> describing the failure.
    /// </returns>
    public async Task<Result<AuthResponse, LoginError>> LoginAsync(string username, string password)
    {
        User? user = await _db.Users.SingleOrDefaultAsync(u => u.Username == username);
        string hashToVerify = user is null ? _dummyHash : user.PasswordHash;
        if (!BCrypt.Net.BCrypt.Verify(PreHash(password), hashToVerify) || user is null)
            return LoginError.InvalidCredentials;

        string accessJwt = CreateAccessJwt(user.Id);
        string refreshTokenBase64Url = await AssignNewRefreshToken(user.Id);

        AuthResponse resp = new(accessJwt, refreshTokenBase64Url);
        return resp;
    }

    /// <summary>
    /// Refreshes a logged in user's session by validating the refresh token and issuing new access JWT and refresh token.
    /// </summary>
    /// <returns>
    /// A Result containing the created <see cref="RefreshSessionResponse"/> on success, or a
    /// <see cref="RefreshTokenError"/> describing the failure.
    /// </returns>
    public async Task<Result<RefreshSessionResponse, RefreshTokenError>> RefreshSessionAsync(
        string refreshTokenBase64Url
    )
    {
        var result = await VerifyRefreshToken(refreshTokenBase64Url);
        if (!result.IsSuccess)
            return result.Error!;

        RefreshToken refreshToken = result.Value!;

        string newAccessJwt = CreateAccessJwt(refreshToken.UserId);
        string newRefreshTokenBase64Url = await AssignNewRefreshToken(refreshToken.UserId);

        RefreshSessionResponse resp = new(newAccessJwt, newRefreshTokenBase64Url);
        return resp;
    }

    public async Task RevokeRefreshTokenAsync(long userId)
    {
        await _db.RefreshTokens.Where(r => r.UserId == userId).ExecuteDeleteAsync();
    }

    // creates a new refresh token and assigns it to a given userId, if the user already has a token, it gets replaced.
    // returns a base64url encoded refresh token
    private async Task<string> AssignNewRefreshToken(int userId)
    {
        var now = DateTimeOffset.UtcNow;

        byte[] rawRefreshToken = RandomNumberGenerator.GetBytes(_config.RefreshTokenSizeBytes);
        byte[] refreshTokenHash = SHA256.HashData(rawRefreshToken);
        DateTimeOffset refreshTokenExpiresAt = now.AddDays(_config.RefreshTokenLifetimeDays);
        await _db.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO refresh_tokens(token_hash, expires_at, user_id)
            VALUES ({refreshTokenHash}, {refreshTokenExpiresAt}, {userId})
            ON CONFLICT (user_id) DO UPDATE SET
                token_hash = EXCLUDED.token_hash,
                expires_at = EXCLUDED.expires_at,
                created_at = now()
            """
        );
        return Base64Url.EncodeToString(rawRefreshToken);
    }

    // bcrypt only reads the first 72 bytes of the password, so anything longer gets silently
    // truncated. sha256 first to get a fixed size digest, then base64 it because bcrypt also stops
    // at the first null byte and a raw digest can contain one.
    private static string PreHash(string password)
    {
        byte[] digest = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(digest);
    }

    private string CreateAccessJwt(long userId)
    {
        long accessTokenExpiresAt = DateTimeOffset
            .UtcNow.AddMinutes(_config.AccessTokenLifetimeMinutes)
            .ToUnixTimeSeconds();
        return _jwtHandler.GenerateToken(userId, accessTokenExpiresAt);
    }

    private async Task<Result<RefreshToken, RefreshTokenError>> VerifyRefreshToken(
        string refreshTokenBase64Url
    )
    {
        byte[] rawRefreshToken = new byte[_config.RefreshTokenSizeBytes];
        if (
            !Base64Url.TryDecodeFromChars(
                refreshTokenBase64Url,
                rawRefreshToken,
                out int bytesWritten
            )
            || bytesWritten != _config.RefreshTokenSizeBytes
        )
            return RefreshTokenError.InvalidFormat;

        byte[] refreshTokenHash = SHA256.HashData(rawRefreshToken);
        RefreshToken? refreshToken = await _db.RefreshTokens.FindAsync(refreshTokenHash);
        if (refreshToken is null)
            return RefreshTokenError.NotFound;

        if (refreshToken.ExpiresAt < DateTime.UtcNow)
            return RefreshTokenError.Expired;

        return refreshToken;
    }
}
