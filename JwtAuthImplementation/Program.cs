using DotNetEnv;
using FluentValidation;
using JwtAuthImplementation.Auth;
using JwtAuthImplementation.Auth.Dtos.Validation;
using JwtAuthImplementation.Auth.JwtHandling;
using JwtAuthImplementation.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
string? DB_CONN_STRING = builder.Configuration["Db:ConnString"];
string? SECRET_KEY = builder.Configuration["Auth:SecretKey"];
if (DB_CONN_STRING is null || SECRET_KEY is null)
    throw new InvalidOperationException("env vars not set.");

builder.Host.UseSerilog(
    (context, config) =>
        config
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.File(
                "logs/logs.txt",
                restrictedToMinimumLevel: LogEventLevel.Information,
                rollingInterval: RollingInterval.Day
            )
);

builder.Services.AddControllers();

// gets variables from appsettings and .env
builder.Services.AddOptions<AuthConfig>().Bind(builder.Configuration.GetSection("Auth"));

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(DB_CONN_STRING));
builder.Services.AddSingleton(_ => new JwtHandler(SECRET_KEY, TimeProvider.System));
builder.Services.AddScoped<AuthService>();

// validators
// registrate all validators, not just LoginRequestValidator?? wtf is an assembly idr get this
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddOpenApi();

var app = builder.Build();

// validate AuthConfig
var authConfig = app.Services.GetRequiredService<IOptions<AuthConfig>>().Value;
var validator = new AuthConfigValidator();
var result = validator.Validate(authConfig);
if (!result.IsValid)
    throw new Exception("authConfig not read correctly.");

app.UseExceptionHandler(error =>
    error.Run(async context =>
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
    })
);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseMiddleware<JwtAuthenticationMiddleware>();
app.MapControllers();
app.Run();
