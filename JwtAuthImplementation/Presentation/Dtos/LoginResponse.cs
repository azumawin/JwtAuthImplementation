namespace JwtAuthImplementation.Auth.Dtos;

public record AuthResponse(string AccessJwt, string RefreshToken);
