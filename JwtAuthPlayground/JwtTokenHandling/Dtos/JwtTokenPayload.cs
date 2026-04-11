namespace JwtAuthPlayground.JwtTokenHandling.Dtos;

public record JwtTokenPayload(string Iss, long Sub, long Exp, long Iat);
