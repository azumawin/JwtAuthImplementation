using FluentValidation;

namespace JwtAuthImplementation.Auth;

public class AuthConfigValidator : AbstractValidator<AuthConfig>
{
    public AuthConfigValidator()
    {
        // check for 0 because if not set that's what an int defaults to.
        RuleFor(ac => ac.SecretKey).NotEmpty();
        RuleFor(ac => ac.AccessTokenLifetimeMinutes).NotEqual(0);
        RuleFor(ac => ac.RefreshTokenLifetimeDays).NotEqual(0);
        RuleFor(ac => ac.RefreshTokenSizeBytes).NotEqual(0);
        RuleFor(ac => ac.MaxUsernameLength).NotEqual(0);
        RuleFor(ac => ac.MinPasswordLength).NotEqual(0);
        RuleFor(ac => ac.MaxPasswordLength).NotEqual(0);
    }
}
