using JwtAuthImplementation.Auth.JwtHandling;
using JwtAuthImplementation.Auth.JwtHandling.Dtos;

namespace JwtAuthImplementation.Auth;

public class JwtAuthenticationMiddleware(RequestDelegate _next)
{
    public async Task Invoke(HttpContext context, JwtHandler _jwtHandler)
    {
        // the scheme is case insensitive per rfc 7235, and a token sent with no scheme at all
        // isnt valid either, so strip the prefix
        const string scheme = "Bearer ";
        string authorization = context.Request.Headers.Authorization.ToString();
        string jwt = authorization.StartsWith(scheme, StringComparison.OrdinalIgnoreCase)
            ? authorization[scheme.Length..].Trim()
            : "";

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
