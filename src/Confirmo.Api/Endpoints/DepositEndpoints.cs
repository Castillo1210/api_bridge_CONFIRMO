using System.Globalization;
using Confirmo.Api.Data;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Models.Entities;
using Confirmo.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Confirmo.Api.Endpoints;

public static class DepositEndpoints
{
    public static void MapDepositEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/deposits")
            .RequireAuthorization()
            .WithTags("Deposits");
        
        group.MapPost("/", async (
            [FromForm] DepositCreateRequest request,
            HttpContext http,
            AppDbContext context,
            IStorageService storage,
            IRedisQueueService redisQueue,
            ISignalRNotificationService notifications,
            ILogger<Program> logger) =>
        {
            var userId = Guid.Parse(http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null) return Results.Unauthorized();

            byte[] imageBytes;
            try { imageBytes = Convert.FromBase64String(request.ImagenBase64); }
            catch { return Results.BadRequest(new { error = "ImagenBase64 inválida" }); }

            // 1. Subir a GCS
            var objectName = await storage.UploadVoucherAsync(user.EmpresaId, userId, imageBytes,  "image/jpeg");

            var deposit = new Deposito
            {
                Id = Guid.NewGuid(),
                Cliente = request.Cliente,
                BancoId = Guid.TryParse(request.BancoId, out var bId) ? bId : null,
                ImagenVoucher = objectName,
                EmpresaId = Guid.TryParse(request.EmpresaId, out var eId) ? eId : null,
                SucursalId = user.SucursalId,
                VendedorId = userId,
                Estado = "recibido",
                FechaRegistro = DateTimeOffset.UtcNow
            };

            context.Depositos.Add(deposit);
            await context.SaveChangesAsync();

            // 3. Publicar en Redis Queue para procesamiento asíncrono
            await redisQueue.PublishAsync("deposit:process:queue", new
            {
                deposit_id = deposit.Id.ToString(),
                object_name = objectName,
                banco_id = request.BancoId,
                empresa_id = request.EmpresaId,
                cliente = request.Cliente,
                retry_count = 0
            });

            await notifications.NotifyDepositReceived(userId, deposit.Id);
            await notifications.NotifyPanelNewDeposit(new PanelDepositSummary(
                DepositId: deposit.Id,
                NumeroOperacion: deposit.NumeroOperacion,
                Cliente: deposit.Cliente,
                Monto: deposit.Monto,
                Moneda: deposit.Moneda,
                Estado: deposit.Estado,
                FechaRegistro: deposit.FechaRegistro,
                Banco: null,
                Sucursal: null,
                VendedorNombre: user.FullName
            ));

            return Results.Ok(new { depositId = deposit.Id });
        })
        .DisableAntiforgery()
        .Accepts<DepositCreateRequest>("multipart/form-data");

        group.MapGet("/{id:guid}", async (Guid id, HttpContext http, AppDbContext context) =>
        {
            var userId = Guid.Parse(http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var deposit = await context.Depositos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id && d.VendedorId == userId);

            return deposit is not null ? Results.Ok(MapToResponse(deposit)) : Results.NotFound();
        });
        
        // GET: Listar depósitos
        group.MapGet("/", async (
            HttpContext http,
            AppDbContext context,
            [FromQuery] string? cliente,
            [FromQuery] decimal? montoMin,
            [FromQuery] decimal? montoMax,
            [FromQuery] string? estado,
            [FromQuery] DateTimeOffset? desde,
            [FromQuery] DateTimeOffset? hasta,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var userId = Guid.Parse(http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var query = context.Depositos.AsNoTracking().Where(d => d.VendedorId == userId);

            if (!string.IsNullOrEmpty(cliente))
            {
                query = query.Where(d => d.Cliente != null && EF.Functions.ILike(d.Cliente, $"%{cliente}"));
            }
            if (montoMin.HasValue)
            {
                query = query.Where(d => d.Monto >= montoMin.Value);
            }
            if (montoMax.HasValue)
            {
                query = query.Where(d => d.Monto <= montoMax.Value);
            }

            if (!string.IsNullOrEmpty(estado)) query = query.Where(d => d.Estado == estado);
            if (desde.HasValue) query = query.Where(d => d.FechaRegistro >= desde.Value);
            if (hasta.HasValue) query = query.Where(d => d.FechaRegistro <= hasta.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(d => d.FechaRegistro)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DepositListResponse(
                    d.Id, d.NumeroOperacion, d.Cliente, d.Monto, d.Moneda, d.FechaRegistro, d.Estado, d.NumeroOperacionBanco, d.FechaDeposito)).ToListAsync();

            return Results.Ok(new DepositListPagedResponse(items, total, page, pageSize));
        });

        group.MapGet("/bancos", async (AppDbContext context) =>
        {
            var bancos = await context.Bancos
                .AsNoTracking()
                .Where(b => b.Activo)
                .Select(b => new BancoResponse(b.Id, b.Nombre, b.Codigo))
                .ToListAsync();

            return Results.Ok(bancos);
        });
        
        // POST: Confirmar depósito (finanzas/admin)
        group.MapPost("/{id:guid}/confirm", async (
            Guid id,
            HttpContext http,
            [FromBody] ConfirmDepositRequest? request,
            AppDbContext context,
            ISignalRNotificationService notifications,
            IFCMNotificationService fcm,
            ILogger<Program> logger) =>
        {
            var userIdClaim = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Results.Unauthorized();

            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null || (user.Rol != "finanzas" && user.Rol != "admin"))
                return Results.Forbid();

            var deposit = await context.Depositos
                .Include(d => d.Empresa)
                .Include(d => d.Sucursal)
                .Include(d => d.Banco)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deposit == null) return Results.NotFound(new { error = "Depósito no encontrado" });

            if (deposit.Estado != "validado" && deposit.Estado != "requiere_revision")
            {
                return Results.BadRequest(new { 
                    error = "Solo se pueden confirmar depósitos en estado 'validado' o 'requiere_revision'",
                    estadoActual = deposit.Estado
                });
            }

            var oldStatus = deposit.Estado;
            deposit.Estado = "confirmado";
            deposit.FechaValidacion = DateTimeOffset.UtcNow;
            deposit.ValidadoPor = userId;
            if (request?.Observaciones != null)
                deposit.Observaciones = request.Observaciones;

            await context.SaveChangesAsync();

            var notification = new DepositConfirmedNotification(
                DepositId: deposit.Id,
                Estado: "confirmado",
                ReferenceNumber: deposit.NumeroOperacionBanco ?? deposit.NumeroOperacion,
                Empresa: deposit.Empresa?.Nombre ?? "",
                Sucursal: deposit.Sucursal?.Nombre ?? "",
                Banco: deposit.Banco?.Nombre ?? "",
                Anexo: deposit.Anexo ?? "",
                FechaDeposito: deposit.FechaDeposito ?? DateOnly.FromDateTime(DateTime.UtcNow),
                NumeroOperacion: deposit.NumeroOperacionBanco ?? deposit.NumeroOperacion,
                Importe: deposit.Monto.ToString(CultureInfo.InvariantCulture),
                Moneda: deposit.Moneda
            );

            await notifications.NotifyDepositConfirmed(deposit.VendedorId, deposit.Id, notification);
            await notifications.NotifyPanelDepositStatusChanged(deposit.Id, "confirmado", oldStatus);

            var vendedor = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == deposit.VendedorId);
            if (vendedor?.FcmToken != null)
            {
                try 
                {
                    await fcm.SendDepositConfirmedAsync(vendedor.FcmToken, notification);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error enviando FCM, pero depósito confirmado");
                }
            }

            logger.LogInformation("Depósito confirmado por finanzas");

            return Results.Ok(new ConfirmDepositResponse(true, "Depósito confirmado exitosamente", notification));
        })
        .RequireAuthorization()
        .WithTags("Deposits")
        .WithSummary("Confirmar depósito manualmente (Solo Finanzas/Admin)")
        .WithDescription("Confirma manualmente un depósito validado. Solo roles: finanzas, admin.");
        
        // PUT: Regularizar depósito rechazado
        group.MapPut("/{id:guid}/regularize", async (
            Guid id,
            HttpContext http,
            [FromForm] RegularizeDepositRequest request,
            AppDbContext context,
            IStorageService storage,
            IRedisQueueService redisQueue,
            ISignalRNotificationService notifications,
            ILogger<Program> logger
        ) =>
        {
            var userId = Guid.Parse(http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null)
                return Results.Unauthorized();
            
            var deposit = await context.Depositos
                .Include(d => d.Empresa)
                .Include(d => d.Sucursal)
                .Include(d => d.Banco)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deposit == null) return Results.NotFound(new { error = "Depósito no encontrado" });

            if (deposit.Estado != "rechazado")
            {
                return Results.BadRequest(new
                {
                    error = "Solo se pueden regularizar depósitos en estado 'rechazado'",
                    estadoActual = deposit.Estado
                });
            }

            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(request.ImagenBase64);
            }
            catch
            {
                return Results.BadRequest(new { error = "ImagenBase64 inválida" });
            }

            if (imageBytes.Length < 1024)
            {
                return Results.BadRequest(new { error = "La imagen es demasiado pequeña. Debe ser al menos 1KB." });
            }

            var objectName = await storage.UploadVoucherAsync(user.EmpresaId, userId, imageBytes, "image/jpeg");

            var oldStatus = deposit.Estado;
            deposit.ImagenVoucher = objectName;
            deposit.Estado = "recibido";
            deposit.MotivoRechazo = null;
            deposit.FechaValidacion = null;
            deposit.ErrorIds = Array.Empty<Guid>();
            deposit.WarningIds = Array.Empty<Guid>();

            await context.SaveChangesAsync();

            await redisQueue.PublishAsync("deposit:process:queue", new
            {
                deposit_id = deposit.Id.ToString(),
                object_name = objectName,
                banco_id = deposit.BancoId?.ToString(),
                empresa_id = deposit.EmpresaId?.ToString(),
                cliente = deposit.Cliente,
                retry_count = 0
            });

            await notifications.NotifyDepositReceived(userId, deposit.Id);
            await notifications.NotifyPanelDepositStatusChanged(deposit.Id, "recibido", oldStatus);
            
            logger.LogInformation("Depósito {DepositId} regularizado por usuario {UserId}", deposit.Id, userId);

            return Results.Ok(new
            {
                depositId = deposit.Id,
                estado = "recibido",
                message = "Depósito regularizado. Será procesado nuevamente."
            });
        })
        .DisableAntiforgery()
        .RequireAuthorization()
        .WithSummary("Regularizar un depósito rechazado")
        .WithDescription("Permite al vendedor re-subir la imagen de un depósito rechazado para re-procesarlo.");
    }

    private static DepositResponse MapToResponse(Deposito d) => new(
        d.Id, d.NumeroOperacion, d.Cliente, d.Monto, d.Moneda, d.FechaRegistro,
        d.ImagenVoucher, d.Anexo, d.NumeroOperacionBanco, d.FechaDeposito,
        d.Estado, d.Observaciones, d.MotivoRechazo, d.FechaValidacion,
        d.EmpresaId, d.BancoId, d.SucursalId, d.VendedorId,
        d.ReferenciaCliente, d.DatosOcr, d.RucCliente
    );
}