namespace JwtAuthPlayground.JwtTokenHandling.Dtos;

public record JwtTokenPayload(long Sub, long Exp, long Iat, string Iss = "JwtAuthPlayground");
