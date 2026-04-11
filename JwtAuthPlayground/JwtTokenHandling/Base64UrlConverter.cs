using System.Text;
using System.Text.Json;

namespace JwtAuthPlayground.JwtTokenHandling;

public static class Base64UrlConverter
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Encodes an object as a Base64Url string.
    /// </summary>
    /// <param name="obj">The object to encode.</param>
    /// <returns>A Base64Url-encoded string.</returns>
    public static string ToBase64Url(object obj)
    {
        string json = JsonSerializer.Serialize(obj, _serializerOptions);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        string base64 = Convert.ToBase64String(bytes);
        string base64url = Base64ToBase64Url(base64);

        return base64url;
    }

    /// <summary>
    /// Encodes a byte array as a Base64Url string.
    /// </summary>
    /// <param name="bytes">The byte array to encode.</param>
    /// <returns>A Base64Url-encoded string.</returns>
    public static string ToBase64Url(byte[] bytes)
    {
        string base64 = Convert.ToBase64String(bytes);
        string base64url = Base64ToBase64Url(base64);

        return base64url;
    }

    /// <summary>
    /// Decodes a Base64Url string into an object.
    /// </summary>
    /// <param name="base64url">The string to decode.</param>
    /// <param name="objectType">The object type to deserialize into.</param>
    /// <returns>An object representation of the data encoded in Base64Url.</returns>
    public static T? FromBase64Url<T>(string base64url)
    {
        string base64 = Base64UrlToBase64(base64url);
        byte[] bytes = Convert.FromBase64String(base64);
        string json = Encoding.UTF8.GetString(bytes);
        T? obj = JsonSerializer.Deserialize<T>(json, _serializerOptions);

        return obj;
    }

    /// <summary>
    /// Convert a base64url string to base64.
    /// </summary>
    /// <param name="base64url">Base64Url string to convert.</param>
    /// <returns>Base64 string representation of the original string.</returns>
    private static string Base64UrlToBase64(string base64url)
    {
        // refer to RFC 4648 section 4
        int paddingCount = (4 - (base64url.Length % 4)) % 4;
        string padding = new('=', paddingCount);
        string base64 = (base64url + padding).Replace('-', '+').Replace('_', '/');
        return base64;
    }

    private static string Base64ToBase64Url(string base64)
    {
        // refer to RFC 4648 section 5
        return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
