using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using JwtAuthPlayground.JwtTokenHandling.Dtos;

namespace JwtAuthPlayground.JwtTokenHandling;

public class JwtTokenHandler
{
    private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Generates a JWT token as defined in RFC 7519.
    /// </summary>
    /// <param name="userId">Id of the user that the token belongs to.</param>
    /// <param name="exp">Expiration date in unix timestamp (seconds since 1970-01-01 UTC).</param>
    /// <param name="iat">Issued at date in unix timestamp (seconds since 1970-01-01 UTC).</param>
    /// <returns>JWT token.</returns>
    public string GenerateToken(long userId, long exp, long iat)
    {
        var header = new JwtTokenHeader("HS256", "JWT");
        var payload = new JwtTokenPayload("JwtAuthPlayground", userId, exp, iat);

        string header64url = Base64UrlEncode(header);
        string payload64url = Base64UrlEncode(payload);

        byte[] keyBytes = Encoding.UTF8.GetBytes("my-secret-key-should-come-from-env");
        byte[] dataBytes = Encoding.UTF8.GetBytes(header64url + "." + payload64url);

        var signatureBytes = HMACSHA256.HashData(keyBytes, dataBytes);
        var signature64url = Base64UrlEncode(signatureBytes);

        var jwtToken = header64url + "." + payload64url + "." + signature64url;

        return jwtToken;
    }

    /// <summary>
    /// Encodes an object as a Base64Url string.
    /// </summary>
    /// <param name="obj">The object to encode.</param>
    /// <returns>A Base64Url-encoded string.</returns>
    public string Base64UrlEncode(object obj)
    {
        var json = JsonSerializer.Serialize(obj, _serializerOptions);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        string base64 = Convert.ToBase64String(bytes);

        // convert base64 to base64url (url and filename safe alphabet) since that's what JWT's use
        // refer to RFC 4648 section 5
        var base64url = base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');

        return base64url;
    }

    /// <summary>
    /// Encodes a byte array as a Base64Url string.
    /// </summary>
    /// <param name="bytes">The byte array to encode.</param>
    /// <returns>A Base64Url-encoded string.</returns>
    public string Base64UrlEncode(byte[] bytes)
    {
        string base64 = Convert.ToBase64String(bytes);

        // convert base64 to base64url (url and filename safe alphabet) since that's what JWT's use
        // refer to RFC 4648 section 5
        var base64url = base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');

        return base64url;
    }
}
