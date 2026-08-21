using System.Security.Claims;
using Confirmo.Api.Data;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Confirmo.Api.Endpoints;

public static class ZavuPlantillaEndpoints
{
    public static void MapZavuPlantillaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/zavu-plantillas")
            .RequireAuthorization()
            .WithTags("ZavuPlantillas");

        // GET: listar plantillas activas (para el dropdown del panel de Avisos)
        group.MapGet("/", async (HttpContext http, AppDbContext context) =>
        {
            var userId = GetUserId(http);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null || user.Rol != "admin")
                return Results.Forbid();

            var plantillas = await context.ZavuPlantillas
                .AsNoTracking()
                .Where(z => z.Activo)
                .OrderBy(z => z.Nombre)
                .Select(z => new ZavuPlantillaResponse(z.Id, z.Nombre, z.Codigo, z.TemplateId, z.Activo, z.CreatedAt))
                .ToListAsync();

            return Results.Ok(plantillas);
        });

        // POST: crear una plantilla nueva (solo admin)
        group.MapPost("/", async (CreateZavuPlantillaRequest request, HttpContext http, AppDbContext context) =>
        {
            var userId = GetUserId(http);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null || user.Rol != "admin")
                return Results.Forbid();

            if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Codigo) || string.IsNullOrWhiteSpace(request.TemplateId))
                return Results.BadRequest(new { error = "Nombre, código y templateId son obligatorios" });

            var existe = await context.ZavuPlantillas.AnyAsync(z => z.Codigo == request.Codigo);
            if (existe)
            {
                return Results.BadRequest(new { error = "Ya existe una plantilla con ese código" });
            }

            var plantilla = new ZavuPlantilla
            {
                Nombre = request.Nombre,
                Codigo = request.Codigo,
                TemplateId = request.TemplateId,
                CreadoPor = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                Activo = true
            };

            context.ZavuPlantillas.Add(plantilla);
            await context.SaveChangesAsync();

            return Results.Ok(new { id = plantilla.Id });
        });

        // PUT: desactivar (solo admin)
        group.MapPut("/{id:guid}/desactivar", async (Guid id, HttpContext http, AppDbContext context) =>
        {
            var userId = GetUserId(http);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null || user.Rol != "admin")
                return Results.Forbid();

            var plantilla = await context.ZavuPlantillas.FirstOrDefaultAsync(z => z.Id == id);
            if (plantilla == null) return Results.NotFound();

            plantilla.Activo = false;
            await context.SaveChangesAsync();

            return Results.Ok(new { id = plantilla.Id, activo = false });
        });
    }

    private static Guid GetUserId(HttpContext http) => Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}