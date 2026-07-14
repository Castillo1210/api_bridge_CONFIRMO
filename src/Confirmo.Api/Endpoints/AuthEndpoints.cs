using System.Security.Claims;
using Confirmo.Api.Data;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Confirmo.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequest request, IAuthService auth) =>
        {
            var outcome = await auth.LoginAsync(request);
            return outcome.Failure switch
            {
                LoginFailure.None => Results.Ok(outcome.Response),
                LoginFailure.DeviceMismatch => Results.Json(
                    new { message = "Ya hay una sesion activa en otro dispositivo. Cierra sesion en ese dispositivo primero para continuar."},
                    statusCode: 409
                ),
                _ => Results.Unauthorized()
            };
        });

        group.MapPost("/logout", async (HttpContext http, IAuthService auth) =>
        {
            var userIdClaim = http.User.FindFirst("sub") ?? http.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            await auth.LogoutAsync(userId);
            return Results.Ok(new { success = true });
        }).RequireAuthorization();

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

        group.MapPut("/fcm-token", async (UpdateFcmTokenRequest request, HttpContext http, AppDbContext context) =>
        {
            var userIdClaim = http.User.FindFirst("sub") ?? http.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var user = await context.Profiles.FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null) return Results.NotFound();

            user.FcmToken = request.Token;
            await context.SaveChangesAsync();

            return Results.Ok(new { success = true });
        }).RequireAuthorization();
    }
}