using DotNetEnv;
using JwtAuthPlayground.Data;
using JwtAuthPlayground.JwtTokenHandling;
using JwtAuthPlayground.Middleware;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Scalar.AspNetCore;

Env.Load();
string? DB_CONN_STRING = Environment.GetEnvironmentVariable("DB_CONN_STRING");
string? SECRET_KEY = Environment.GetEnvironmentVariable("SECRET_KEY");
if (DB_CONN_STRING is null || SECRET_KEY is null)
    throw new InvalidOperationException("env vars not set.");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(DB_CONN_STRING));
builder.Services.AddSingleton(_ => new JwtTokenHandler(SECRET_KEY));
builder.Services.AddOpenApi();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseMiddleware<JwtAuthenticationMiddleware>();
app.MapControllers();
app.Run();
