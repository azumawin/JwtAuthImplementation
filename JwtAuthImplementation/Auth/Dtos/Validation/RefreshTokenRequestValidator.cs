using System.Buffers.Text;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace JwtAuthImplementation.Auth.Dtos.Validation;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator(IOptions<AuthConfig> options)
    {
        AuthConfig config = options.Value;

        RuleFor(rtr => rtr.RefreshTokenBase64Url)
            .NotEmpty()
            .Must(token => Base64Url.IsValid(token));
    }
}
