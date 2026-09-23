namespace JwtAuthImplementation.Domain.Jwt;

public record JwtPayload(long Sub, long Exp, long Iat, string Iss = "JwtAuthImplementation");
