namespace JwtAuthImplementation.Domain.Config;

public class AuthConfig()
{
    public string SecretKey { get; set; } = null!;
    public int AccessTokenLifetimeMinutes { get; set; }
    public int RefreshTokenLifetimeDays { get; set; }
    public int RefreshTokenSizeBytes { get; set; }
    public int MinUsernameLength { get; set; }
    public int MaxUsernameLength { get; set; }
    public int MinPasswordLength { get; set; }
    public int MaxPasswordLength { get; set; }
}
