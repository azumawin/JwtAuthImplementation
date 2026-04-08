using System.ComponentModel.DataAnnotations;

namespace JwtAuthPlayground.Validation;

public class NotInPastAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is DateTime date && date < DateTime.UtcNow)
        {
            return new ValidationResult("Date cannot be in the past.");
        }

        return ValidationResult.Success;
    }
}
