using JwtAuthPlayground.Data;
using JwtAuthPlayground.Dtos;
using JwtAuthPlayground.JwtTokenHandling;
using JwtAuthPlayground.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace JwtAuthPlayground.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext _db) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UserSummary>> Register(RegisterUserRequest request)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User { Username = request.Username, PasswordHash = passwordHash };

        try
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pgEx
                && pgEx.SqlState == PostgresErrorCodes.UniqueViolation
            )
        {
            return Conflict("User already exists in the db.");
        }

        var resp = new UserSummary(user.Id, user.Username, user.CreatedAt);
        return StatusCode(201, resp);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginUserResponse>> Login(LoginUserRequest request)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == request.Username);

        // vulnerability: an attacker can know whether the account with this username exists or not based on response time
        // but i don't want to be schizo
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid username or password");

        var now = DateTimeOffset.UtcNow;
        long issuedAt = now.ToUnixTimeSeconds();
        long expiresAt = now.AddMinutes(10).ToUnixTimeSeconds();
        var jwtAccessToken = JwtTokenHandler.GenerateToken(user.Id, expiresAt, issuedAt);
        var resp = new LoginUserResponse(jwtAccessToken);
        return Ok(resp);
    }
}
