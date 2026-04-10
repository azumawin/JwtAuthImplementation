using JwtAuthPlayground.JwtTokenHandling.Dtos;
using JwtAuthPlayground.Models;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthPlayground.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching",
    ];

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable
            .Range(1, 5)
            .Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)],
            })
            .ToArray();
    }

    [HttpGet("TestingSmth")]
    public IActionResult TestingSmth()
    {
        HttpContext.Items.TryGetValue("isValidJwt", out var valid);
        HttpContext.Items.TryGetValue("payload", out var pl);
        return Ok(new { valid, pl });
    }
}
