namespace JwtAuthImplementation.Auth.Dtos;

public record AuthResponse(string AccessJwt, string RefreshTokenBase64Url);
