using DotNetEnv;
using JwtAuthPlayground.Data;
using JwtAuthPlayground.JwtTokenHandling;
using JwtAuthPlayground.Middleware;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Scalar.AspNetCore;

Env.Load();
string DB_CONN_STRING = Environment.GetEnvironmentVariable("DB_CONN_STRING");
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(DB_CONN_STRING));
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
