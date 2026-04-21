using JwtAuthImplementation.Auth.JwtHandling;
using JwtAuthImplementation.Auth.JwtHandling.Dtos;

namespace JwtAuthImplementation.Auth;

public class JwtAuthenticationMiddleware(RequestDelegate _next)
{
    public async Task Invoke(HttpContext context, JwtHandler _jwtHandler)
    {
        var jwt = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        bool result = _jwtHandler.VerifyJwt(jwt);
        context.Items.Add("isValidJwt", result);

        if (!result)
        {
            await _next(context);
            return;
        }

        JwtPayload? payload = JwtHandler.TryGetPayload(jwt);
        context.Items.Add("payload", payload);

        await _next(context);
    }
}
