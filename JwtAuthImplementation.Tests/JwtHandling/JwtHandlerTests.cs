using JwtAuthImplementation.Auth.JwtHandling;
using JwtAuthImplementation.Auth.JwtHandling.Dtos;
using JwtAuthImplementation.Domain;
using Microsoft.Extensions.Time.Testing;

namespace JwtAuthImplementation.Tests.JwtHandling;

public class JwtHandlerTests
{
    private static readonly FakeTimeProvider _fakeTimeProvider = new(
        DateTimeOffset.MinValue.AddMinutes(10)
    );
    private static readonly JwtHandler _jwtHandler = new("sigmaboy", _fakeTimeProvider);

    [Fact]
    public void GenerateToken_GeneratesCorrectToken()
    {
        // coupled to SecretKey
        string token = _jwtHandler.GenerateToken(1, 2);
        string expected =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOjEsImV4cCI6MiwiaWF0IjotNjIxMzU1OTYyMDAsImlzcyI6Ikp3dEF1dGhJbXBsZW1lbnRhdGlvbiJ9.Gzg1PhGQttooyL8N1YAwdY-d7gMn3hM1PX7JlOAhvas";
        Assert.Equal(expected, token);
    }

    [Theory]
    [InlineData(long.MinValue, long.MinValue)]
    [InlineData(0, 0)]
    [InlineData(long.MaxValue, long.MaxValue)]
    public void GenerateToken_RoundTrip(long userId, long exp)
    {
        string token = _jwtHandler.GenerateToken(userId, exp);

        JwtHeader? parsedHeader = JwtHandler.TryGetHeader(token);
        Assert.NotNull(parsedHeader);
        Assert.Equal("HS256", parsedHeader.Alg);
        Assert.Equal("JWT", parsedHeader.Typ);

        JwtPayload? roundTripPayload = JwtHandler.TryGetPayload(token);
        Assert.NotNull(roundTripPayload);
        Assert.Equal(userId, roundTripPayload.Sub);
        Assert.Equal(exp, roundTripPayload.Exp);
    }

    [Fact]
    public void IsValid_RejectsExpired()
    {
        long exp = DateTimeOffset.MinValue.ToUnixTimeSeconds();
        string token = _jwtHandler.GenerateToken(2, exp);
        bool result = _jwtHandler.VerifyJwt(token);
        Assert.False(result);
    }

    [Fact]
    public void IsValid_AcceptsNotExpired()
    {
        long exp = _fakeTimeProvider.GetUtcNow().AddMinutes(1).ToUnixTimeSeconds();
        string token = _jwtHandler.GenerateToken(3, exp);
        bool result = _jwtHandler.VerifyJwt(token);
        Assert.True(result);
    }

    [Fact]
    public void IsValid_RejectsTamperedHeader()
    {
        long exp = _fakeTimeProvider.GetUtcNow().AddMinutes(1).ToUnixTimeSeconds();
        string token = _jwtHandler.GenerateToken(4, exp);
        bool result = _jwtHandler.VerifyJwt(token);
        Assert.True(result);

        // tamper with token by changing a value in header
        JwtHeader fakeHeader = new("RAND", "JWT");

        // build fake token
        string fakeHeaderBase64Url = JsonBase64UrlEncoder.Encode(fakeHeader);
        string[] parts = token.Split('.');
        string modifiedToken = fakeHeaderBase64Url + "." + parts[1] + "." + parts[2];
        bool newResult = _jwtHandler.VerifyJwt(modifiedToken);

        Assert.False(newResult);
    }

    [Fact]
    public void IsValid_RejectsTamperedPayload()
    {
        long userId = 5;
        long exp = _fakeTimeProvider.GetUtcNow().AddMinutes(1).ToUnixTimeSeconds();
        string token = _jwtHandler.GenerateToken(userId, exp);
        bool result = _jwtHandler.VerifyJwt(token);
        Assert.True(result);

        // tamper with token by changing a value in payload
        long expFake = _fakeTimeProvider.GetUtcNow().AddMinutes(2).ToUnixTimeSeconds();
        long iat = _fakeTimeProvider.GetUtcNow().ToUnixTimeSeconds();
        JwtPayload fakePayload = new(userId, expFake, iat);

        // build fake token
        string fakePayloadBase64Url = JsonBase64UrlEncoder.Encode(fakePayload);
        string[] parts = token.Split('.');
        string modifiedToken = parts[0] + "." + fakePayloadBase64Url + "." + parts[2];
        bool newResult = _jwtHandler.VerifyJwt(modifiedToken);

        Assert.False(newResult);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not.a.jwt")]
    [InlineData("not.a.jwt.either")]
    public void IsValid_DoesntThrowOnBadInput(string jwtToken)
    {
        bool result = _jwtHandler.VerifyJwt(jwtToken);
        Assert.False(result);
    }
}
