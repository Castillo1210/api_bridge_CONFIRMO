using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Services;

namespace Confirmo.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequest request, IAuthService auth) =>
        {
            var result = await auth.LoginAsync(request);
            return result is not null ? Results.Ok(result) : Results.Unauthorized();
        });

        group.MapPost("/refresh", async (RefreshRequest request, IAuthService auth) =>
        {
            var result = await auth.RefreshAsync(request.RefreshToken);
            return result is not null ? Results.Ok(result) : Results.Unauthorized();
        });
    }
}