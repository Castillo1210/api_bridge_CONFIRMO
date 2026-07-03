using System.Security.Claims;
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

        group.MapPost("/change-password", async (ChangePasswordRequest request, HttpContext http, IAuthService auth) =>
        {
            // 1. Buscamos el claim usando el estándar abreviado "sub" o el largo de Microsoft
            var userIdClaim = http.User.FindFirst("sub") 
                            ?? http.User.FindFirst(ClaimTypes.NameIdentifier);

            // 2. Si no lo encuentra, evitamos el crash devolviendo un 401 limpio
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return Results.Json(new { message = "No se encontró el identificador del usuario en el token (sub/NameIdentifier)" }, statusCode: 401);
            }        

            // 3. Parseamos de forma segura el Guid
            if (!Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Json(new { message = "El identificador del usuario no tiene un formato Guid válido" }, statusCode: 400);
            }

            //var userId = Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await auth.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        }).RequireAuthorization();
    }
}