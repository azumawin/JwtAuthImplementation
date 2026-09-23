using JwtAuthImplementation.Shared;

namespace JwtAuthImplementation.Application;

public record RegisterError(string Code, string Description, string? StackTrace = null)
    : Error(Code, Description, StackTrace)
{
    public static readonly RegisterError UsernameTaken = new(
        "Register.UsernameTaken",
        "Username is already taken."
    );
}

public record LoginError(string Code, string Description, string? StackTrace = null)
    : Error(Code, Description, StackTrace)
{
    public static readonly LoginError InvalidCredentials = new(
        "Login.InvalidCredentials",
        "Incorrect username or password."
    );
}

public record RefreshTokenError(string Code, string Description, string? StackTrace = null)
    : Error(Code, Description, StackTrace)
{
    public static readonly RefreshTokenError NotFound = new(
        "RefreshToken.NotFound",
        "Refresh token not found."
    );
    public static readonly RefreshTokenError Expired = new(
        "RefreshToken.Expired",
        "Refresh token expired."
    );
    public static readonly RefreshTokenError InvalidFormat = new(
        "RefreshToken.InvalidFormat",
        "Refresh token is either not properly Base64Url encoded or something else went wrong."
    );
}
