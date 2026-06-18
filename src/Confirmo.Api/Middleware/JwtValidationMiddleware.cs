using Confirmo.Api.Services;

namespace Confirmo.Api.Middleware;

public class JwtValidationMiddleware
{
    private readonly RequestDelegate _next;

    public JwtValidationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Query["access_token"].FirstOrDefault();

        if (string.IsNullOrEmpty(token) && context.Request.Headers.Authorization.Count > 0)
        {
            token = context.Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer", "");
        }

        if (!string.IsNullOrEmpty(token))
        {
            var authService = context.RequestServices.GetRequiredService<IAuthService>();
            var principal = authService.ValidateToken(token);

            if (principal != null)
            {
                context.User = principal;
            }
        }

        await _next(context);
    }
}