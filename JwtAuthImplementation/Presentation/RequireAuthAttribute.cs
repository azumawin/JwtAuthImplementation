using JwtAuthImplementation.Auth.JwtHandling.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JwtAuthImplementation.Presentation;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class RequireAuthAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.Items["payload"] is not JwtPayload payload)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
    }
}
