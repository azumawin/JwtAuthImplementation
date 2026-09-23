/*
 * this service is supposed to be called by the controller only, so it is assumed that the data is already validated by fluentvalidation
 */
using System.Buffers.Text;
using System.Security.Cryptography;
using JwtAuthImplementation.Domain;
using JwtAuthImplementation.Shared;
using Microsoft.Extensions.Options;

namespace JwtAuthImplementation.Application;

public class AuthService(
    IUserRepository _userRepository,
    IRefreshTokenRepository _refreshTokenRepository,
    IOptions<AuthConfig> _options,
    JwtService _jwtHandler
)
{
    private static readonly string _dummyHash = BCrypt.Net.BCrypt.HashPassword(
        "DummyHashToPreventTimingAttacks"
    );
    private readonly AuthConfig config = _options.Value;

    /// <summary>
    /// Registers a new user into the database.
    /// </summary>
    /// <remarks>
    /// Assumes username and password are already validated by the FluentValidation pipeline.
    /// </remarks>
    /// <returns>
    /// A Result containing the registered <see cref="User"/> on success, or a
    /// <see cref="RegisterError"/> describing the failure.
    /// </returns>
    public async Task<Result<User, RegisterError>> RegisterAsync(
        string username,
        string password
    )
    {
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        // idk what to do here: my domain model User has an id field, but i cant know that until the db hands me back one and i need a model for passing user to database, so i'm using the one from domain layer, since if i were to use the one scaffolded into infrastructure then application would depend on infrastructure and thats wrong so i have to use the domain one or create a dto, but my question is, what is the purpose a user model in the domain in this app? or should i just remove id from it
        User user = new() {Username = username, PasswordHash = passwordHash };
        var result = await _userRepository.AddAsync(user);
        if (result == UserRepositoryAddResult.Success)
        {
            return user;
        }
        else if (result == UserRepositoryAddResult.UsernameConflict)
        {
            return RegisterError.UsernameTaken;
        }
        else
        {
            throw new UnhandledFunctionPathException();
        }
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
        User? user = await _userRepository.GetByUsernameAsync(username);
        string hashToVerify = user is null ? _dummyHash : user.PasswordHash;
        if (!BCrypt.Net.BCrypt.Verify(password, hashToVerify) || user is null)
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

    public async Task RevokeRefreshTokenAsync(long userId, CancellationToken ct = default)
    {
        await _refreshTokenRepository.RevokeRefreshTokenAsync(userId, ct);
    }

    // should be called something else i think
    // creates a new refresh token and assigns it to a given userId, if the user already has a token, it gets replaced.
    // returns a base64url encoded refresh token
    private async Task<string> AssignNewRefreshToken(int userId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        byte[] rawRefreshToken = RandomNumberGenerator.GetBytes(config.RefreshTokenSizeBytes);
        byte[] refreshTokenHash = SHA256.HashData(rawRefreshToken);
        DateTimeOffset refreshTokenExpiresAt = now.AddDays(config.RefreshTokenLifetimeDays);
        await _refreshTokenRepository.ReplaceRefreshTokenAsync(
            userId,
            refreshTokenHash,
            refreshTokenExpiresAt,
            ct
        );
        return Base64Url.EncodeToString(rawRefreshToken);
    }

    // should be in jwt handler
    private string CreateAccessJwt(long userId)
    {
        long accessTokenExpiresAt = DateTimeOffset
            .UtcNow.AddMinutes(config.AccessTokenLifetimeMinutes)
            .ToUnixTimeSeconds();
        return _jwtHandler.GenerateToken(userId, accessTokenExpiresAt);
    }

    private async Task<Result<RefreshToken, RefreshTokenError>> VerifyRefreshToken(
        string refreshTokenBase64Url
    )
    {
        byte[] rawRefreshToken = new byte[config.RefreshTokenSizeBytes];
        if (
            !Base64Url.TryDecodeFromChars(
                refreshTokenBase64Url,
                rawRefreshToken,
                out int bytesWritten
            )
            || bytesWritten != config.RefreshTokenSizeBytes
        )
            return RefreshTokenError.InvalidFormat;

        byte[] refreshTokenHash = SHA256.HashData(rawRefreshToken);
        RefreshToken? refreshToken = ;
        if (refreshToken is null)
            return RefreshTokenError.NotFound;

        if (refreshToken.ExpiresAt < DateTime.UtcNow)
            return RefreshTokenError.Expired;

        return refreshToken;
    }
}
