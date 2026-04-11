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

        JwtTokenPayload? payload = JwtTokenHandler.GetPayload<JwtTokenPayload>(jwtToken);
        context.Items.Add("payload", payload);

        await _next(context);
    }
}
