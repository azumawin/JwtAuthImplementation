using System.Buffers.Text;
using System.Text;
using System.Text.Json;

namespace JwtAuthPlayground.JwtTokenHandling;

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
    public static string Encode<T>(T obj)
    {
        string json = JsonSerializer.Serialize(obj, _serializerOptions);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        return Base64Url.EncodeToString(bytes);
    }

    /// <summary>
    /// Decode a Base64Url encoded JSON string.
    /// </summary>
    /// <param name="jsonBase64Url">String to decode.</param>
    /// <typeparam name="T">Type that matches the structure of the JSON (the type to deserialize into).</typeparam>
    /// <returns>Object representation of the Base64Url encoded JSON string.</returns>
    public static T? Decode<T>(string jsonBase64Url)
    {
        byte[] jsonBytes = Base64Url.DecodeFromChars(jsonBase64Url);
        return JsonSerializer.Deserialize<T>(jsonBytes, _serializerOptions);
    }
}
