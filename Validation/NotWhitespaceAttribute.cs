using System.ComponentModel.DataAnnotations;

namespace JwtAuthPlayground.Validation;

public class NotWhitespaceAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is string s && string.IsNullOrWhiteSpace(s))
        {
            return new ValidationResult($"{context.DisplayName} must not be blank or whitespace.");
        }

        return ValidationResult.Success;
    }
}
