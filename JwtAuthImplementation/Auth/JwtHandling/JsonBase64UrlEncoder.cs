using System.Buffers.Text;
using System.Text;
using System.Text.Json;

namespace JwtAuthImplementation.Auth.JwtHandling;

// not testing this
public static class JsonBase64UrlEncoder
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Encode an object as a Base64Url JSON string.
    /// </summary>
    /// <param name="obj">Object to encode.</param>
    /// <typeparam name="T">Type of the object.</typeparam>
    /// <returns>Base64Url encoded JSON string.</returns>
    /// <exception cref="NotSupportedException"></exception>
    public static string Encode<T>(T obj)
    {
        string json = JsonSerializer.Serialize(obj, _serializerOptions);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        return Base64Url.EncodeToString(bytes);
    }

    /// <summary>
    /// Decode a Base64Url encoded JSON string into an object of type <see cref="T"/>
    /// </summary>
    /// <param name="jsonBase64Url">JSON string encoded in Base64Url to decode from.</param>
    /// <typeparam name="T">Type to deserialize into.</typeparam>
    /// <returns>Object representation of the Base64Url encoded JSON string or null.</returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="JsonException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    public static T? Decode<T>(string jsonBase64Url)
    {
        byte[] jsonBytes = Base64Url.DecodeFromChars(jsonBase64Url);
        return JsonSerializer.Deserialize<T>(jsonBytes, _serializerOptions);
    }
}
