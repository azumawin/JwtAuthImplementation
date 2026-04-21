using JwtAuthImplementation.Auth.Dtos;
using JwtAuthImplementation.Auth.JwtHandling.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthImplementation.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService _authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UserSummary>> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request.Username, request.Password);
        if (!result.IsSuccess)
        {
            if (result.Error == RegisterError.UsernameTaken)
                return Conflict(result.Error.Description);
            return StatusCode(500, result.Error!.Description);
        }
        return StatusCode(201, result.Value);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Username, request.Password);
        if (!result.IsSuccess)
        {
            if (result.Error == LoginError.InvalidCredentials)
                return Unauthorized(result.Error.Description);
            return StatusCode(500, result.Error!.Description);
        }
        return Ok(result.Value);
    }

    // probably badly named ngl
    [HttpPost("refreshToken")]
    public async Task<ActionResult<RefreshSessionResponse>> RefreshToken(
        RefreshTokenRequest request
    )
    {
        var result = await _authService.RefreshSessionAsync(request.RefreshTokenBase64Url);
        if (!result.IsSuccess)
        {
            if (result.Error == RefreshTokenError.InvalidFormat)
                return BadRequest(result.Error.Description);
            if (result.Error == RefreshTokenError.Expired)
                return Unauthorized(result.Error.Description);
            if (result.Error == RefreshTokenError.NotFound)
                return NotFound(result.Error.Description);
            return StatusCode(500, result.Error!.Description);
        }
        return Ok(result.Value);
    }

    [HttpPost("logout")]
    [RequireAuth]
    public async Task<ActionResult> Logout()
    {
        var payload = (JwtPayload)HttpContext.Items["payload"]!;
        await _authService.RevokeRefreshTokenAsync(payload.Sub);
        return NoContent();
    }
}
