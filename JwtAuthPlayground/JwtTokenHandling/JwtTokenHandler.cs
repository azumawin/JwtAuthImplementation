using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using JwtAuthPlayground.JwtTokenHandling.Dtos;

namespace JwtAuthPlayground.JwtTokenHandling;

// add docstrings and unit tests
public static class JwtTokenHandler
{
    private static readonly byte[] _keyBytes = Encoding.UTF8.GetBytes(
        Environment.GetEnvironmentVariable("SECRET_KEY")!
    );
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };
    private static readonly JwtTokenHeader _header = new("HS256", "JWT");
    private static readonly string _headerJsonBase64Url = ToJsonBase64Url(_header);

    /// <summary>
    /// Generates a JWT token as defined in RFC 7519.
    /// </summary>
    /// <param name="userId">Id of the user that the token belongs to.</param>
    /// <param name="exp">Expiration date in unix timestamp (seconds since 1970-01-01 UTC).</param>
    /// <param name="iat">Issued at date in unix timestamp (seconds since 1970-01-01 UTC).</param>
    /// <returns>JWT token as a string.</returns>
    public static string GenerateToken(long userId, long exp, long iat)
    {
        JwtTokenPayload payload = new("JwtAuthPlayground", userId, exp, iat);

        string payloadJsonBase64Url = ToJsonBase64Url(payload);
        byte[] dataBytes = Encoding.UTF8.GetBytes(
            _headerJsonBase64Url + "." + payloadJsonBase64Url
        );
        byte[] signatureBytes = HMACSHA256.HashData(_keyBytes, dataBytes);
        string signatureBase64Url = Base64Url.EncodeToString(signatureBytes);

        string jwtToken =
            _headerJsonBase64Url + "." + payloadJsonBase64Url + "." + signatureBase64Url;
        return jwtToken;
    }

    public static bool IsValidToken(string jwtToken)
    {
        string[] parts = jwtToken.Split('.');
        if (parts.Length != 3)
            return false;

        string headerJsonBase64Url = parts[0];
        string payloadJsonBase64Url = parts[1];
        string signatureBase64Url = parts[2];

        byte[] dataBytes = Encoding.UTF8.GetBytes(headerJsonBase64Url + "." + payloadJsonBase64Url);
        byte[] computedSignatureBytes = HMACSHA256.HashData(_keyBytes, dataBytes);
        string computedSignatureBase64Url = Base64Url.EncodeToString(computedSignatureBytes);

        // maybe comparing strings isnt smart, could cause some edge case errors, should prob compare bytes
        if (signatureBase64Url != computedSignatureBase64Url)
            return false;

        // a token with a valid signature but no payload is treated as invalid
        JwtTokenPayload? payload = FromJsonBase64Url<JwtTokenPayload>(payloadJsonBase64Url);
        if (payload is null)
            return false;

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (payload.Exp < now)
            return false;

        return true;
    }

    private static string ToJsonBase64Url<T>(T obj)
    {
        string json = JsonSerializer.Serialize(obj, _serializerOptions);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        return Base64Url.EncodeToString(bytes);
    }

    private static T? FromJsonBase64Url<T>(string jsonBase64Url)
    {
        byte[] jsonBytes = Base64Url.DecodeFromChars(jsonBase64Url);
        return JsonSerializer.Deserialize<T>(jsonBytes, _serializerOptions);
    }
}
