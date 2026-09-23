namespace JwtAuthImplementation.Shared;

public record Error(string Code, string Description, string? StackTrace = null)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error Unexpected(string code, string description) =>
        new(code, description, Environment.StackTrace);
}
