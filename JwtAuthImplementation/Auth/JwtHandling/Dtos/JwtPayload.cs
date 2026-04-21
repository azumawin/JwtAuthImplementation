namespace JwtAuthImplementation.Auth.JwtHandling.Dtos;

public record JwtPayload(long Sub, long Exp, long Iat, string Iss = "JwtAuthImplementation");
