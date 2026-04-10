using JwtAuthPlayground.JwtTokenHandling;
using JwtAuthPlayground.JwtTokenHandling.Dtos;

namespace JwtAuthPlayground.Middleware;

public class JwtAuthenticationMiddleware(RequestDelegate _next)
{
    public async Task Invoke(HttpContext context)
    {
        var jwtToken = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        bool result = JwtTokenHandler.IsValidToken(jwtToken);
        context.Items.Add("isValidJwt", result);

        if (!result)
        {
            await _next(context);
            return;
        }

        string[] parts = jwtToken.Split('.');
        JwtTokenPayload? payload = Base64UrlConverter.FromBase64Url<JwtTokenPayload>(parts[1]);
        context.Items.Add("payload", payload);

        await _next(context);
    }
}
