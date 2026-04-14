using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using JwtAuthPlayground.JwtTokenHandling.Dtos;

namespace JwtAuthPlayground.JwtTokenHandling;

public class JwtTokenHandler(string _secretKey, TimeProvider _timeProvider)
{
    private readonly byte[] _secretKeyBytes = Encoding.UTF8.GetBytes(_secretKey);
    private static readonly string _headerJsonBase64Url = JsonBase64UrlEncoder.Encode(
        new JwtTokenHeader("HS256", "JWT")
    );

    /// <summary>
    /// Generates a JWT as defined in RFC 7519.
    /// </summary>
    /// <param name="userId">Id of the user that the token belongs to.</param>
    /// <param name="exp">Expiration date in unix timestamp (seconds since 1970-01-01 UTC).</param>
    /// <returns>JWT as a string.</returns>
    public string GenerateToken(long userId, long exp)
    {
        long iat = _timeProvider.GetUtcNow().ToUnixTimeSeconds();
        JwtTokenPayload payload = new(userId, exp, iat);
        string payloadJsonBase64Url = JsonBase64UrlEncoder.Encode(payload);
        byte[] dataBytes = Encoding.UTF8.GetBytes(
            _headerJsonBase64Url + "." + payloadJsonBase64Url
        );
        byte[] signatureBytes = HMACSHA256.HashData(_secretKeyBytes, dataBytes);
        string signatureBase64Url = Base64Url.EncodeToString(signatureBytes);

        string jwtToken =
            _headerJsonBase64Url + "." + payloadJsonBase64Url + "." + signatureBase64Url;
        return jwtToken;
    }

    /// <summary>
    /// Verifies that a token is valid by taking it apart and re-computing it using the secret key.
    /// </summary>
    /// <param name="jwtToken">Token to verify.</param>
    /// <returns>Bool result.</returns>
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
            byte[] computedSignatureBytes = HMACSHA256.HashData(_secretKeyBytes, dataBytes);
            string computedSignatureBase64Url = Base64Url.EncodeToString(computedSignatureBytes);

            // maybe comparing strings isnt smart, could cause some edge case errors, should prob compare bytes
            if (signatureBase64Url != computedSignatureBase64Url)
                return false;

            // header must match the hardcoded one, since this version doesn't support any other hashing algorithms
            if (headerJsonBase64Url != _headerJsonBase64Url)
                return false;

            // payload must be a valid JSON, parseable into the JwtTokenPayload type
            JwtTokenPayload? payload = JsonBase64UrlEncoder.Decode<JwtTokenPayload>(
                payloadJsonBase64Url
            );
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
            // could be something else than a parsing error and i could just catch the specific exceptions
            // but i think it's better to never have this method throw at all.
            return false;
        }
    }

    // get header of a jwt token as a JwtTokenHeader object
    public static JwtTokenHeader? TryGetHeader(string jwtToken)
    {
        string[] parts = jwtToken.Split('.');
        if (parts.Length != 3)
            throw new FormatException("Token is not a valid JWT.");

        return JsonBase64UrlEncoder.Decode<JwtTokenHeader>(parts[0]);
    }

    // get payload of a jwt token as a JwtTokenPayload object
    public static JwtTokenPayload? TryGetPayload(string jwtToken)
    {
        string[] parts = jwtToken.Split('.');
        if (parts.Length != 3)
            throw new FormatException("Token is not a valid JWT.");

        return JsonBase64UrlEncoder.Decode<JwtTokenPayload>(parts[1]);
    }
}
