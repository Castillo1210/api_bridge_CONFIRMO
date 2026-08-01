using System.Security.Claims;
using Confirmo.Api.Data;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Models.Entities;
using Confirmo.Api.Services;
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
                    a.EsRecurrente, a.Frecuencia, a.HoraEjecucion, a.DiaSemana, a.DiaMes, a.ProximaEjecucion, a.UltimaEjecucion,
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
                    a.EsRecurrente, a.Frecuencia, a.HoraEjecucion, a.DiaSemana, a.DiaMes, a.ProximaEjecucion, a.UltimaEjecucion,
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

        //POST: Subir imagen/PDF
        group.MapPost("/media", async (UploadAvisoMediaRequest request, HttpContext http, AppDbContext context, IStorageService storage) =>
        {
            var userId = GetUserId(http);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null || user.Rol != "admin")
                return Results.Forbid();

            if (string.IsNullOrWhiteSpace(request.ImagenBase64))
                return Results.BadRequest(new { error = "Imagen requerida" });

            byte[] bytes;
            try
            {
                var cleaned = request.ImagenBase64.Contains(',') ? request.ImagenBase64.Split(',')[1] : request.ImagenBase64;
                bytes = Convert.FromBase64String(cleaned);
            }
            catch
            {
                return Results.BadRequest(new { error = "Imagen inválida" });
            }

            var contentType = request.ContentType ?? "image/jpeg";
            var objectName = await storage.UploadAvisoMediaAsync(bytes, contentType);

            var galeriaItem = new AvisoImagenGaleria
            {
                ObjectName = objectName,
                ContentType = contentType,
                Nombre = string.IsNullOrWhiteSpace(request.Nombre) ? null : request.Nombre.Trim(),
                CreadoPor = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                Activo = true
            };

            context.AvisosImagenesGaleria.Add(galeriaItem);
            await context.SaveChangesAsync();

            return Results.Ok(new { mediaUrl = objectName, tipoMedia = contentType, galeriaId = galeriaItem.Id });
        });

        //PUT: Editar un aviso (solo admin)
        group.MapPut("/{id:guid}", async (Guid id, UpdateAvisoRequest request, HttpContext http, AppDbContext context) =>
        {
            var userId = GetUserId(http);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null || user.Rol != "admin")
                return Results.Forbid();

            var aviso = await context.Avisos.FirstOrDefaultAsync(a => a.Id == id);
            if (aviso == null) return Results.NotFound();

            if (string.IsNullOrWhiteSpace(request.Titulo) || string.IsNullOrWhiteSpace(request.MensajeTexto))
                return Results.BadRequest(new { error = "Titulo y mensaje son obligatorios" });

            if (request.RolesDestino == null || request.RolesDestino.Length == 0)
                return Results.BadRequest(new { error = "Debe indicar al menos un rol activo" });

            if (!request.EnviarApp && !request.EnviarWhatsapp && !request.EnviarEmail)
                return Results.BadRequest(new { error = "Debe activar al menos un canal" });

            if (request.EsRecurrente && string.IsNullOrWhiteSpace(request.Frecuencia))
                return Results.BadRequest(new { error = "Un aviso recurrente necesita una frecuencia" });

            aviso.Titulo = request.Titulo;
            aviso.MensajeTexto = request.MensajeTexto;
            aviso.MediaUrl = request.MediaUrl;
            aviso.TipoMedia = request.TipoMedia;
            aviso.RolesDestino = request.RolesDestino;
            aviso.EnviarApp = request.EnviarApp;
            aviso.EnviarWhatsapp = request.EnviarWhatsapp;
            aviso.EnviarEmail = request.EnviarEmail;
            aviso.AsuntoEmail = request.AsuntoEmail;
            aviso.EsRecurrente = request.EsRecurrente;
            aviso.Frecuencia = request.Frecuencia;
            aviso.HoraEjecucion = request.HoraEjecucion;
            aviso.DiaSemana = request.DiaSemana;
            aviso.DiaMes = request.DiaMes;

            if (aviso.Estado == "programado" && request.ProgramadoPara != null)
            {
                aviso.ProximaEjecucion = request.ProgramadoPara;
            }

            await context.SaveChangesAsync();

            return Results.Ok(new { id = aviso.Id });
        });

        //POST: Reenviar un aviso ya registrado
        group.MapPost("/{id:guid}/reenviar", async (Guid id, ReenviarAvisoRequest? request, HttpContext http, AppDbContext context) =>
        {
            var userId = GetUserId(http);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null || user.Rol != "admin")
                return Results.Forbid();

            var aviso = await context.Avisos.FirstOrDefaultAsync(a => a.Id == id);
            if (aviso == null) return Results.NotFound();

            aviso.Activo = true;
            aviso.Estado = "programado";
            aviso.ProximaEjecucion = request?.ProgramadoPara ?? DateTimeOffset.UtcNow;

            await context.SaveChangesAsync();

            return Results.Ok(new { id = aviso.Id, proximaEjecucion = aviso.ProximaEjecucion });
        });

        //GET: LIstar las imagenes de la galeria
        group.MapGet("/galeria", async (HttpContext http, AppDbContext context) =>
        {
            var userId = GetUserId(http);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null || user.Rol != "admin")
                return Results.Forbid();

            var imagenes = await context.AvisosImagenesGaleria
                .AsNoTracking()
                .Where(i => i.Activo)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new AvisoImagenGaleriaResponse(i.Id, i.ObjectName, i.ContentType, i.Nombre, i.CreatedAt))
                .ToListAsync();

            return Results.Ok(imagenes);
        });

        //GET: URL firmada de una imagen de la galeria
        group.MapGet("/galeria/{id:guid}/imagen", async (Guid id, HttpContext http, AppDbContext context, IStorageService storage) =>
        {
            var userId = GetUserId(http);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null || user.Rol != "admin")
                return Results.Forbid();

            var imagen = await context.AvisosImagenesGaleria.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
            if (imagen == null) return Results.NotFound();

            var signedUrl = await storage.GetSignedUrlAsync(imagen.ObjectName);
            return Results.Redirect(signedUrl);
        });

        //DELETE: Quitar una imagen de la galeria
        group.MapDelete("/galeria/{id:guid}", async (Guid id, HttpContext http, AppDbContext context) =>
        {
            var userId = GetUserId(http);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null || user.Rol != "admin")
                return Results.Forbid();

            var imagen = await context.AvisosImagenesGaleria.FirstOrDefaultAsync(i => i.Id == id);
            if (imagen == null) return Results.NotFound();

            imagen.Activo = false;
            await context.SaveChangesAsync();

            return Results.Ok(new { id = imagen.Id, active = false });
        });

        //GET: Obtener URL firmada por imagen
        group.MapGet("/{id:guid}/media", async (Guid id, HttpContext http, AppDbContext context, IStorageService storage) =>
        {
            var userId = GetUserId(http);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null) return Results.Forbid();

            var aviso = await context.Avisos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (aviso == null || string.IsNullOrEmpty(aviso.MediaUrl))
                return Results.NotFound();

            var signedUrl = await storage.GetSignedUrlAsync(aviso.MediaUrl);
            return Results.Redirect(signedUrl);
        });
    }

    private static Guid GetUserId(HttpContext http) => Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}