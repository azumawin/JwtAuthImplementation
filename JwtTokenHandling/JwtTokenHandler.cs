using System.Security.Cryptography;
using System.Text;
using JwtAuthPlayground.JwtTokenHandling.Dtos;

namespace JwtAuthPlayground.JwtTokenHandling;

public static class JwtTokenHandler
{
    private static readonly byte[] _keyBytes = Encoding.UTF8.GetBytes(
        Environment.GetEnvironmentVariable("SECRET_KEY")
    );

    /// <summary>
    /// Generates a JWT token as defined in RFC 7519.
    /// </summary>
    /// <param name="userId">Id of the user that the token belongs to.</param>
    /// <param name="exp">Expiration date in unix timestamp (seconds since 1970-01-01 UTC).</param>
    /// <param name="iat">Issued at date in unix timestamp (seconds since 1970-01-01 UTC).</param>
    /// <returns>JWT token as a string.</returns>
    public static string GenerateToken(long userId, long exp, long iat)
    {
        var header = new JwtTokenHeader("HS256", "JWT");
        var payload = new JwtTokenPayload("JwtAuthPlayground", userId, exp, iat);

        string header64url = Base64UrlConverter.ToBase64Url(header);
        string payload64url = Base64UrlConverter.ToBase64Url(payload);

        byte[] dataBytes = Encoding.UTF8.GetBytes(header64url + "." + payload64url);

        var signatureBytes = HMACSHA256.HashData(_keyBytes, dataBytes);
        var signature64url = Base64UrlConverter.ToBase64Url(signatureBytes);

        var jwtToken = header64url + "." + payload64url + "." + signature64url;

        return jwtToken;
    }

    public static bool IsValidToken(string jwtToken)
    {
        string[] parts = jwtToken.Split('.');
        if (parts.Length != 3)
            return false;

        string header64url = parts[0];
        string payload64url = parts[1];
        string signature64url = parts[2];

        // verify signature
        byte[] dataBytes = Encoding.UTF8.GetBytes(header64url + "." + payload64url);
        string computedSignature64url = Base64UrlConverter.ToBase64Url(
            HMACSHA256.HashData(_keyBytes, dataBytes)
        );
        // maybe i should use string.Comparison?
        if (signature64url != computedSignature64url)
            return false;

        JwtTokenPayload? payload = Base64UrlConverter.FromBase64Url<JwtTokenPayload>(payload64url);
        if (payload is null)
            // log
            return false;

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (payload.Exp < now)
            return false;

        return true;
    }
}
