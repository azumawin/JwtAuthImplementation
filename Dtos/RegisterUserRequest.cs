using System.ComponentModel.DataAnnotations;
using JwtAuthPlayground.Validation;

namespace JwtAuthPlayground.Dtos;

public record RegisterUserRequest(
    [Required, MaxLength(50), NotWhitespace] string Username,
    [Required, MaxLength(300), NotWhitespace] string Password
);
