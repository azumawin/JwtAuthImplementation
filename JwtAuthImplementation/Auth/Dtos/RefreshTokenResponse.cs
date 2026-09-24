namespace JwtAuthImplementation.Auth.Dtos;

public record RefreshSessionResponse(string AccessJwt, string RefreshToken);
