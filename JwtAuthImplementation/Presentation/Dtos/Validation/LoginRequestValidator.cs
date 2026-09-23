using FluentValidation;
using Microsoft.Extensions.Options;

namespace JwtAuthImplementation.Auth.Dtos.Validation;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator(IOptions<AuthConfig> options)
    {
        AuthConfig config = options.Value;

        RuleFor(lr => lr.Username).NotEmpty().MaximumLength(config.MaxUsernameLength);
        RuleFor(lr => lr.Password)
            .NotEmpty()
            .MinimumLength(config.MinPasswordLength)
            .MaximumLength(config.MaxPasswordLength);
    }
}
