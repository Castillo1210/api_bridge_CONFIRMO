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
            IPythonWorkerClient worker,
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
                NumeroOperacion = request.NumeroOperacion,
                Cliente = request.Cliente,
                BancoId = Guid.TryParse(request.BancoId, out var bId) ? bId : null,
                Anexo = request.Anexo,
                ImagenVoucher = objectName,
                EmpresaId = user.EmpresaId,
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
                empresa_id = user.EmpresaId.ToString(),
                cliente = request.Cliente,
                retry_count = 0
            });

            return Results.Ok();
        })
        .DisableAntiforgery()
        .Accepts<DepositCreateRequest>("multipart/form-data");

        group.MapGet("/{id:guid}", async (Guid id, HttpContext http, AppDbContext context) =>
        {
            var userId = Guid.Parse(http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var deposit = await context.Depositos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id && d.VendedorId == userId);

            return deposit is not null ? Results.Ok(MapToResponse(deposit)) : Results.NotFound();
        });

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
    }

    private static DepositResponse MapToResponse(Deposito d) => new(
        d.Id, d.NumeroOperacion, d.Cliente, d.Monto, d.Moneda, d.FechaRegistro,
        d.ImagenVoucher, d.Anexo, d.NumeroOperacionBanco, d.FechaDeposito,
        d.Estado, d.Observaciones, d.MotivoRechazo, d.FechaValidacion,
        d.EmpresaId, d.BancoId, d.SucursalId, d.VendedorId,
        d.ReferenciaCliente, d.DatosOcr, d.RucCliente
    );
}