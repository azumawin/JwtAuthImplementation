using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using JwtAuthPlayground.JwtTokenHandling.Dtos;

namespace JwtAuthPlayground.JwtTokenHandling;

// add docstrings and unit tests
// double check this class since i made it not static anymore (generateToken and IsValidToken methods too)
public class JwtTokenHandler(string secretKey)
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };
    private readonly byte[] secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
    private static readonly JwtTokenHeader _header = new("HS256", "JWT");
    private static readonly string _headerJsonBase64Url = ToJsonBase64Url(_header);

    /// <summary>
    /// Generates a JWT as defined in RFC 7519.
    /// </summary>
    /// <param name="userId">Id of the user that the token belongs to.</param>
    /// <param name="exp">Expiration date in unix timestamp (seconds since 1970-01-01 UTC).</param>
    /// <param name="iat">Issued at date in unix timestamp (seconds since 1970-01-01 UTC).</param>
    /// <returns>JWT as a string.</returns>
    public string GenerateToken(long userId, long exp, long iat)
    {
        JwtTokenPayload payload = new(userId, exp, iat);

        string payloadJsonBase64Url = ToJsonBase64Url(payload);
        byte[] dataBytes = Encoding.UTF8.GetBytes(
            _headerJsonBase64Url + "." + payloadJsonBase64Url
        );
        byte[] signatureBytes = HMACSHA256.HashData(secretKeyBytes, dataBytes);
        string signatureBase64Url = Base64Url.EncodeToString(signatureBytes);

        string jwtToken =
            _headerJsonBase64Url + "." + payloadJsonBase64Url + "." + signatureBase64Url;
        return jwtToken;
    }

    public bool IsValidToken(string jwtToken)
    {
        try
        {
            string[] parts = jwtToken.Split('.');
            if (parts.Length != 3)
                return false;

            string headerJsonBase64Url = parts[0];
            string payloadJsonBase64Url = parts[1];
            string signatureBase64Url = parts[2];

            byte[] dataBytes = Encoding.UTF8.GetBytes(
                headerJsonBase64Url + "." + payloadJsonBase64Url
            );
            byte[] computedSignatureBytes = HMACSHA256.HashData(secretKeyBytes, dataBytes);
            string computedSignatureBase64Url = Base64Url.EncodeToString(computedSignatureBytes);

            // maybe comparing strings isnt smart, could cause some edge case errors, should prob compare bytes
            if (signatureBase64Url != computedSignatureBase64Url)
                return false;

            // header must match the hardcoded one
            if (headerJsonBase64Url != _headerJsonBase64Url)
                return false;

            // payload must be a valid JSON, parseable into the JwtTokenPayload type
            JwtTokenPayload? payload = FromJsonBase64Url<JwtTokenPayload>(payloadJsonBase64Url);
            if (payload is null)
                return false;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (payload.Exp < now)
                return false;

            return true;
        }
        catch (Exception)
        {
            // if something failed while checking whether the token is valid, it's probably not.
            // could be something else than a parsing error and i could just catch the specific exceptions
            // but i think it's better to never have this method throw at all.
            return false;
        }
    }

    public static T? GetPayload<T>(string jwtToken)
    {
        string[] parts = jwtToken.Split('.');
        if (parts.Length != 3)
            throw new FormatException("Token is not a valid JWT.");

        return FromJsonBase64Url<T>(parts[1]);
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
