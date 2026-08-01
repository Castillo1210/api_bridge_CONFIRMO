using System.Security.Claims;
using Confirmo.Api.Data;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Confirmo.Api.Endpoints;

public static class AvisoEndpoints
{
    public static void MapAvisoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/avisos")
            .RequireAuthorization()
            .WithTags("Avisos");

        // POST: Creación de avisos (solo admin)
        group.MapPost("/", async (
            CreateAvisoRequest request,
            HttpContext http,
            AppDbContext context
        ) =>
        {
            var userId = GetUserId(http);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null || user.Rol != "admin")
                return Results.Forbid();

            if (string.IsNullOrWhiteSpace(request.Titulo) || string.IsNullOrWhiteSpace(request.MensajeTexto))
                return Results.BadRequest(new { error = "Titulo y mensaje son obligatorios" });

            if (request.RolesDestino == null || request.RolesDestino.Length == 0)
                return Results.BadRequest(new { error = "Debe indicar al menos un rol activo" });
            
            if (!request.EnviarApp && !request.EnviarWhatsapp && !request.EnviarEmail)
                return Results.BadRequest(new { error = "Debe activar al menos un canal" });

            if (request.EsRecurrente && string.IsNullOrWhiteSpace(request.Frecuencia))
                return Results.BadRequest(new { error = "Un aviso recurrente necesita una frecuencia" });

            var aviso = new Aviso
            {
                Titulo = request.Titulo,
                MensajeTexto = request.MensajeTexto,
                MediaUrl = request.MediaUrl,
                TipoMedia = request.TipoMedia,
                RolesDestino = request.RolesDestino,
                EnviarApp = request.EnviarApp,
                EnviarWhatsapp = request.EnviarWhatsapp,
                EnviarEmail = request.EnviarEmail,
                AsuntoEmail = request.AsuntoEmail,
                EsRecurrente = request.EsRecurrente,
                Frecuencia = request.Frecuencia,
                HoraEjecucion = request.HoraEjecucion,
                DiaSemana = request.DiaSemana,
                DiaMes = request.DiaMes,
                ProximaEjecucion = request.ProgramadoPara ?? DateTimeOffset.UtcNow,
                CreadoPor = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                Estado = "programado",
                Activo = true
            };

            context.Avisos.Add(aviso);
            await context.SaveChangesAsync();

            return Results.Ok(new { id = aviso.Id });
        });

        //GET: Listar todos los avisos (solo admin, panel de gestión)
        group.MapGet("/", async (HttpContext http, AppDbContext context) =>
        {
            var userId = GetUserId(http);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null || user.Rol != "admin")
                return Results.Forbid();

            var avisos = await context.Avisos
                .AsNoTracking()
                .Include(a => a.Creador)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AvisoResponse(
                    a.Id, a.Titulo, a.MensajeTexto, a.MediaUrl, a.TipoMedia, a.RolesDestino,
                    a.EnviarApp, a.EnviarWhatsapp, a.EnviarEmail, a.AsuntoEmail,
                    a.EsRecurrente, a.Frecuencia, a.ProximaEjecucion, a.UltimaEjecucion,
                    a.Estado, a.Activo, a.Creador != null ? a.Creador.FullName : null, a.CreatedAt
                )).ToListAsync();

            return Results.Ok(avisos);
        });

        //GET: El listado que consume la aplicación móvil
        group.MapGet("/mios", async (HttpContext http, AppDbContext context) =>
        {
            var userId = GetUserId(http);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null) return Results.Forbid();

            var avisos = await context.Avisos
                .AsNoTracking()
                .Where(a => a.Activo && a.EnviarApp && a.RolesDestino.Contains(user.Rol) && a.UltimaEjecucion != null)
                .OrderByDescending(a => a.UltimaEjecucion)
                .Select(a => new AvisoResponse(
                    a.Id, a.Titulo, a.MensajeTexto, a.MediaUrl, a.TipoMedia, a.RolesDestino,
                    a.EnviarApp, a.EnviarWhatsapp, a.EnviarEmail, a.AsuntoEmail,
                    a.EsRecurrente, a.Frecuencia, a.ProximaEjecucion, a.UltimaEjecucion,
                    a.Estado, a.Activo, null, a.CreatedAt
                )).ToListAsync();

            return Results.Ok(avisos);
        });

        //PUT: Desactivar (solo admin)
        group.MapPut("/{id:guid}/desactivar", async (Guid id, HttpContext http, AppDbContext context) =>
        {
            var userId = GetUserId(http);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null || user.Rol != "admin")
                return Results.Forbid();

            var aviso = await context.Avisos.FirstOrDefaultAsync(a => a.Id == id);
            if (aviso == null) return Results.NotFound();

            aviso.Activo = false;
            aviso.ProximaEjecucion = null;
            await context.SaveChangesAsync();

            return Results.Ok(new { id = aviso.Id, activo = false });
        });
    }

    private static Guid GetUserId(HttpContext http) => Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}