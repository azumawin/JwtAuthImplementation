using JwtAuthPlayground.JwtTokenHandling.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JwtAuthPlayground.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class RequireAuthAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var payload = context.HttpContext.Items["payload"] as JwtTokenPayload;
        if (payload is null)
            context.Result = new UnauthorizedResult();
    }
}
