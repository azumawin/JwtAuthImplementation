namespace JwtAuthImplementation.Auth.Dtos;

public record RefreshSessionResponse(string AccessToken, string RefreshToken);
