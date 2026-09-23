using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;

namespace JwtAuthImplementation.Domain.Jwt;

public class JwtService(string _secretKey, TimeProvider _timeProvider)
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };
    private readonly byte[] _secretKeyBytes = Encoding.UTF8.GetBytes(_secretKey);
    private static readonly string headerJsonBase64Url = JsonBase64UrlEncoder.Encode(
        new JwtHeader("HS256", "JWT")
    );

    /// <summary>
    /// Generates a JWT as defined in RFC 7519.
    /// </summary>
    /// <param name="userId">Id of the user that the token belongs to.</param>
    /// <param name="exp">Expiration date in unix timestamp (seconds since 1970-01-01 UTC).</param>
    /// <returns>JWT as a string.</returns>
    public string GenerateJwt(long userId, long exp)
    {
        long iat = _timeProvider.GetUtcNow().ToUnixTimeSeconds();
        JwtPayload payload = new(userId, exp, iat);

        string payloadJsonBase64Url = JsonBase64UrlEncoder.Encode(payload);
        byte[] dataBytes = Encoding.UTF8.GetBytes(headerJsonBase64Url + "." + payloadJsonBase64Url);

        byte[] signatureBytes = HMACSHA256.HashData(_secretKeyBytes, dataBytes);
        string signatureBase64Url = Base64Url.EncodeToString(signatureBytes);

        string jwt = headerJsonBase64Url + "." + payloadJsonBase64Url + "." + signatureBase64Url;
        return jwt;
    }

    // add documentation and then cite RFC 7519
    // change to result type so that it retusn the reaosn why it's an invalid jwt
    public bool IsValidJwt(string jwt)
    {
        try
        {
            // 1
            if (!jwt.Contains('.'))
                return false;

            // 2
            string[] parts = jwt.Split('.', 2);
            string headerBase64Url = parts[0];
            string rest = parts[1];

            // 3
            if (!IsBase64UrlSegment(headerBase64Url))
                return false;
            byte[] octets = Base64Url.DecodeFromChars(headerBase64Url);

            // 4
            if (!Utf8.IsValid(octets))
                return false;
            JwtHeader? header = JsonSerializer.Deserialize<JwtHeader>(octets, _serializerOptions);
            if (header is null)
                return false;

            // 5
        }
        catch (FormatException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    // validate and verify are 2 different things
    /// <summary>
    /// Verifies that a token is a valid - not expired JWT signed with correct signature.
    /// </summary>
    /// <returns>true if JWT is valid and trustable</returns>
    public bool VerifyJwt(string jwt)
    {
        try
        {
            string headerJsonBase64Url = parts[0];
            string payloadJsonBase64Url = parts[1];
            string signatureBase64Url = parts[2];

            byte[] dataBytes = Encoding.UTF8.GetBytes(
                headerJsonBase64Url + "." + payloadJsonBase64Url
            );
            byte[] computedSignatureBytes = HMACSHA256.HashData(_secretKeyBytes, dataBytes);
            string computedSignatureBase64Url = Base64Url.EncodeToString(computedSignatureBytes);

            // maybe comparing strings isnt smart, could cause some edge case errors, should prob compare bytes
            if (signatureBase64Url != computedSignatureBase64Url)
                return false;

            // header must match the hardcoded one, since this version doesn't support any other hashing algorithms
            if (headerJsonBase64Url != JwtService.headerJsonBase64Url)
                return false;

            // payload must be a valid JSON, parseable into the JwtPayload type
            JwtPayload? payload = Decode<JwtPayload>(payloadJsonBase64Url);
            if (payload is null)
                return false;

            long now = _timeProvider.GetUtcNow().ToUnixTimeSeconds();
            if (payload.Exp < now)
                return false;

            return true;
        }
        catch (Exception)
        {
            // if something failed while checking whether the token is valid, it's probably not.
            return false;
        }
    }

    /// <summary>
    /// Attempts to get and parse the payload from a JWT.
    /// </summary>
    /// <param name="jwt"></param>
    /// <returns>JWT payload as a JwtPayload object</returns>
    public static bool TryGetPayload(string jwt, out JwtPayload? payload)
    {
        payload = null;
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length != 3)
                return false;

            payload = Decode<JwtPayload>(parts[1]);
            return payload is not null;
        }
        catch
        {
            return false;
        }
    }

    // used for step 3 in RFC 7519
    private static bool IsBase64UrlSegment(ReadOnlySpan<char> s)
    {
        if (s.IsEmpty)
            return false;
        foreach (char c in s)
            if (!char.IsAsciiLetterOrDigit(c) && c != '-' && c != '_')
                return false;
        return true;
    }

    private static T? Decode<T>(string jsonBase64Url)
    {
        byte[] jsonBytes = Base64Url.DecodeFromChars(jsonBase64Url);
        return JsonSerializer.Deserialize<T>(jsonBytes, _serializerOptions);
    }

    private static string Encode<T>(T obj)
    {
        string json = JsonSerializer.Serialize(obj, _serializerOptions);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        return Base64Url.EncodeToString(bytes);
    }
}
