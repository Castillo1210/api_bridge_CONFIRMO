using System.Security.Claims;
using Confirmo.Api.Data;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Models.Entities;
using Confirmo.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Confirmo.Api.Endpoints;

public static class MasterEndpoints
{
    public static void MapMasterEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/masters")
            .RequireAuthorization()
            .WithTags("Masters");

        // BANCOS

        // GET: listar activos (para dropdowns, cualquier usuario autenticado)
        group.MapGet("/bancos", async (AppDbContext context) =>
        {
            var bancos = await context.Bancos.AsNoTracking()
                .Where(b => b.Activo)
                .Select(b => new BancoResponse(b.Id, b.Nombre, b.Codigo))
                .ToListAsync();

            return Results.Ok(bancos);
        });

        // GET: listar todos (admin/finanzas, incluye inactivos)
        group.MapGet("/bancos/all", async (AppDbContext context, HttpContext http) =>
        {
            if (!IsAdminOrFinanzas(http))
            {
                return Results.Forbid();
            }

            var bancos = await context.Bancos.AsNoTracking()
                .Select(b => new { b.Id, b.Nombre, b.Codigo, b.Activo })
                .ToListAsync();

            return Results.Ok(bancos);
        }); 

        // GET: uno por ID
        group.MapGet("/bancos/{id:guid}", async (Guid id, AppDbContext context) =>
        {
            var banco = await context.Bancos.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);
            return banco is not null ? Results.Ok(new { banco.Id, banco.Nombre, banco.Codigo, banco.Activo }) : Results.NotFound();
        });

        // POST: crear
        group.MapPost("/bancos", async (CreateBancoRequest request, AppDbContext context, HttpContext http) =>
        {
            if (!IsAdmin(http))
            {
                return Results.Forbid();
            }

            var banco = new Banco
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre,
                Codigo = request.Codigo,
                Activo = true
            };
            context.Bancos.Add(banco);
            await context.SaveChangesAsync();
            return Results.Created($"/api/v1/masters/bancos/{banco.Id}", new BancoResponse(banco.Id, banco.Nombre, banco.Codigo));
        });

        // PUT: actualizar
        group.MapPut("/bancos/{id:guid}", async (Guid id, UpdateBancoRequest request, AppDbContext context, HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            var banco = await context.Bancos.FirstOrDefaultAsync(b => b.Id == id);
            if (banco == null) return Results.NotFound();

            banco.Nombre = request.Nombre;
            banco.Codigo = request.Codigo;
            banco.Activo = request.Activo;
            await context.SaveChangesAsync();
            return Results.Ok(new BancoResponse(banco.Id, banco.Nombre, banco.Codigo));
        });

        // DELETE: soft delete
        group.MapDelete("/bancos/{id:guid}", async (Guid id, AppDbContext context, HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            var banco = await context.Bancos.FirstOrDefaultAsync(b => b.Id == id);
            if (banco == null) return Results.NotFound();

            banco.Activo = false;
            await context.SaveChangesAsync();
            return Results.Ok(new { deleted = true, id });
        });

        // ═══════════════════════════════════════════
        // EMPRESAS
        // ═══════════════════════════════════════════

        group.MapGet("/empresas", async (AppDbContext context) =>
        {
            var empresas = await context.Empresas.AsNoTracking()
                .Where(e => e.Activo)
                .Select(e => new EmpresaResponse(e.Id, e.Nombre, e.Logo))
                .ToListAsync();
            return Results.Ok(empresas);
        });

        group.MapGet("/empresas/all", async (AppDbContext context, HttpContext http) =>
        {
            if (!IsAdminOrFinanzas(http)) return Results.Forbid();

            var empresas = await context.Empresas.AsNoTracking()
                .Select(e => new { e.Id, e.Nombre, e.Ruc, e.Logo, e.Activo })
                .ToListAsync();
            return Results.Ok(empresas);
        });

        group.MapGet("/empresas/{id:guid}", async (Guid id, AppDbContext context) =>
        {
            var empresa = await context.Empresas.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
            return empresa is not null
                ? Results.Ok(new { empresa.Id, empresa.Nombre, empresa.Ruc, empresa.Logo, empresa.Activo })
                : Results.NotFound();
        });

        group.MapPost("/empresas", async (CreateEmpresaRequest request, AppDbContext context, HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            var empresa = new Empresa
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre,
                Ruc = request.Ruc,
                Logo = request.Logo,
                Activo = true
            };
            context.Empresas.Add(empresa);
            await context.SaveChangesAsync();
            return Results.Created($"/api/v1/masters/empresas/{empresa.Id}",
                new EmpresaResponse(empresa.Id, empresa.Nombre, empresa.Logo));
        });

        group.MapPut("/empresas/{id:guid}", async (Guid id, UpdateEmpresaRequest request, AppDbContext context, HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            var empresa = await context.Empresas.FirstOrDefaultAsync(e => e.Id == id);
            if (empresa == null) return Results.NotFound();

            empresa.Nombre = request.Nombre;
            empresa.Ruc = request.Ruc;
            empresa.Logo = request.Logo;
            empresa.Activo = request.Activo;
            await context.SaveChangesAsync();
            return Results.Ok(new EmpresaResponse(empresa.Id, empresa.Nombre, empresa.Logo));
        });

        group.MapDelete("/empresas/{id:guid}", async (Guid id, AppDbContext context, HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            var empresa = await context.Empresas.FirstOrDefaultAsync(e => e.Id == id);
            if (empresa == null) return Results.NotFound();

            empresa.Activo = false;
            await context.SaveChangesAsync();
            return Results.Ok(new { deleted = true, id });
        });

        // ═══════════════════════════════════════════
        // SUCURSALES
        // ═══════════════════════════════════════════

        group.MapGet("/sucursales", async (AppDbContext context, [FromQuery] Guid? empresaId) =>
        {
            var query = context.Sucursales.AsNoTracking().Where(s => s.Activo);
            if (empresaId.HasValue)
                query = query.Where(s => s.EmpresaId == empresaId.Value);

            var sucursales = await query
                .Select(s => new SucursalResponse(s.Id, s.EmpresaId, s.Nombre, s.Direccion, s.Activo))
                .ToListAsync();
            return Results.Ok(sucursales);
        });

        group.MapGet("/sucursales/all", async (AppDbContext context, HttpContext http, [FromQuery] Guid? empresaId) =>
        {
            if (!IsAdminOrFinanzas(http)) return Results.Forbid();

            var query = context.Sucursales.AsNoTracking();
            if (empresaId.HasValue)
                query = query.Where(s => s.EmpresaId == empresaId.Value);

            var sucursales = await query
                .Select(s => new SucursalResponse(s.Id, s.EmpresaId, s.Nombre, s.Direccion, s.Activo))
                .ToListAsync();
            return Results.Ok(sucursales);
        });

        group.MapGet("/sucursales/{id:guid}", async (Guid id, AppDbContext context) =>
        {
            var sucursal = await context.Sucursales.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
            return sucursal is not null
                ? Results.Ok(new SucursalResponse(sucursal.Id, sucursal.EmpresaId, sucursal.Nombre, sucursal.Direccion, sucursal.Activo))
                : Results.NotFound();
        });

        group.MapPost("/sucursales", async (CreateSucursalRequest request, AppDbContext context, HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            var empresaExists = await context.Empresas.AnyAsync(e => e.Id == request.EmpresaId && e.Activo);
            if (!empresaExists) return Results.BadRequest(new { error = "Empresa no encontrada o inactiva" });

            var sucursal = new Sucursal
            {
                Id = Guid.NewGuid(),
                EmpresaId = request.EmpresaId,
                Nombre = request.Nombre,
                Direccion = request.Direccion,
                Activo = true
            };
            context.Sucursales.Add(sucursal);
            await context.SaveChangesAsync();
            return Results.Created($"/api/v1/masters/sucursales/{sucursal.Id}",
                new SucursalResponse(sucursal.Id, sucursal.EmpresaId, sucursal.Nombre, sucursal.Direccion, sucursal.Activo));
        });

        group.MapPut("/sucursales/{id:guid}", async (Guid id, UpdateSucursalRequest request, AppDbContext context, HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            var sucursal = await context.Sucursales.FirstOrDefaultAsync(s => s.Id == id);
            if (sucursal == null) return Results.NotFound();

            sucursal.EmpresaId = request.EmpresaId;
            sucursal.Nombre = request.Nombre;
            sucursal.Direccion = request.Direccion;
            sucursal.Activo = request.Activo;
            await context.SaveChangesAsync();
            return Results.Ok(new SucursalResponse(sucursal.Id, sucursal.EmpresaId, sucursal.Nombre, sucursal.Direccion, sucursal.Activo));
        });

        group.MapDelete("/sucursales/{id:guid}", async (Guid id, AppDbContext context, HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            var sucursal = await context.Sucursales.FirstOrDefaultAsync(s => s.Id == id);
            if (sucursal == null) return Results.NotFound();

            sucursal.Activo = false;
            await context.SaveChangesAsync();
            return Results.Ok(new { deleted = true, id });
        });


        // Cuentas bancarias
        group.MapGet("/cuentasbancarias", async (AppDbContext context, HttpContext http, [FromQuery] Guid? empresaId, [FromQuery] Guid? bancoId, [FromQuery] bool? activo) =>
        {
            if (!IsAdminOrFinanzas(http)) return Results.Forbid();

            var query = context.CuentasBancarias.AsNoTracking();
            
            if (empresaId.HasValue) query = query.Where(c => c.EmpresaId == empresaId.Value);
            if (bancoId.HasValue) query = query.Where(c => c.BancoId == bancoId.Value);
            if (activo.HasValue) query = query.Where(c => c.Activo == activo.Value);

            var items = await query
                .Select(c => new CuentaBancariaResponse(c.Id, c.EmpresaId, c.BancoId, c.NumeroCuenta, c.Anexo, c.Activo))
                .ToListAsync();

            return Results.Ok(items);
        });

        group.MapGet("/cuentasbancarias/{id:guid}", async (Guid id, AppDbContext context) =>
        {
            var cuentabancaria = await context.CuentasBancarias.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
            return cuentabancaria is not null ? Results.Ok(new CuentaBancariaResponse(cuentabancaria.Id, cuentabancaria.EmpresaId, cuentabancaria.BancoId, cuentabancaria.NumeroCuenta, cuentabancaria.Anexo, cuentabancaria.Activo)) : Results.NotFound();
        });

        group.MapPost("/cuentasbancarias", async (CreateCuentaBancariaRequest request, AppDbContext context, HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            var empresaExists = await context.Empresas.AnyAsync(e => e.Id == request.EmpresaId && e.Activo);
            if (!empresaExists) return Results.BadRequest(new { error = "Empresa no encontrada o no activa."});

            var bancoExists = await context.Empresas.AnyAsync(b => b.Id == request.BancoId && b.Activo);
            if (!bancoExists) return Results.BadRequest(new { error = "Banco no encontrada o no activa."});

            var cuentabancaria = new CuentaBancaria
            {
                EmpresaId = request.EmpresaId,
                BancoId = request.BancoId,
                NumeroCuenta = request.NumeroCuenta,
                Anexo = request.Anexo,
                Activo = true
            };
            context.CuentasBancarias.Add(cuentabancaria);
            await context.SaveChangesAsync();
            return Results.Created($"/api/v1/masters/cuentasbancarias/{cuentabancaria.Id}", new CuentaBancariaResponse(cuentabancaria.Id, cuentabancaria.EmpresaId, cuentabancaria.BancoId, cuentabancaria.NumeroCuenta, cuentabancaria.Anexo, cuentabancaria.Activo));
        });

        group.MapPut("/cuentasbancarias/{id:guid}", async (Guid id, UpdateCuentaBancariaRequest request, AppDbContext context, HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            var cuentabancaria = await context.CuentasBancarias.FirstOrDefaultAsync(c => c.Id == id);
            if (cuentabancaria == null) return Results.NotFound();

            cuentabancaria.EmpresaId = request.EmpresaId;
            cuentabancaria.BancoId = request.BancoId;
            cuentabancaria.NumeroCuenta = request.NumeroCuenta;
            cuentabancaria.Anexo = request.Anexo;
            cuentabancaria.Activo = request.Activo;
            await context.SaveChangesAsync();
            return Results.Ok(new CuentaBancariaResponse(cuentabancaria.Id, cuentabancaria.EmpresaId, cuentabancaria.BancoId, cuentabancaria.NumeroCuenta, cuentabancaria.Anexo, cuentabancaria.Activo));
        });

        group.MapDelete("/cuentasbancarias/{id:guid}", async (Guid id, AppDbContext context, HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            var cuentabancaria = await context.CuentasBancarias.FirstOrDefaultAsync(c => c.Id == id);
            if (cuentabancaria == null) return Results.NotFound();

            cuentabancaria.Activo = false;
            await context.SaveChangesAsync();
            return Results.Ok(new { deleted = true, id});
        });

        // ═══════════════════════════════════════════
        // TRABAJADORES
        // ═══════════════════════════════════════════

        group.MapGet("/trabajadores", async (AppDbContext context, HttpContext http,
            [FromQuery] Guid? empresaId, [FromQuery] Guid? sucursalId, [FromQuery] bool? activo) =>
        {
            if (!IsAdminOrFinanzas(http)) return Results.Forbid();

            var query = context.Trabajadores.AsNoTracking();
            if (empresaId.HasValue) query = query.Where(t => t.EmpresaId == empresaId.Value);
            if (sucursalId.HasValue) query = query.Where(t => t.SucursalId == sucursalId.Value);
            if (activo.HasValue) query = query.Where(t => t.Activo == activo.Value);

            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TrabajadorResponse(t.Id, t.ProfileId, t.Nombre, t.TelefonoPersonal,
                    t.EmpresaId, t.SucursalId, t.Activo, t.FechaInicio, t.FechaFin))
                .ToListAsync();

            return Results.Ok(items);
        });

        group.MapGet("/trabajadores/{id:guid}", async (Guid id, AppDbContext context, HttpContext http) =>
        {
            if (!IsAdminOrFinanzas(http)) return Results.Forbid();

            var t = await context.Trabajadores.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return t is not null
                ? Results.Ok(new TrabajadorResponse(t.Id, t.ProfileId, t.Nombre, t.TelefonoPersonal,
                    t.EmpresaId, t.SucursalId, t.Activo, t.FechaInicio, t.FechaFin))
                : Results.NotFound();
        });

        group.MapPost("/trabajadores", async (CreateTrabajadorRequest request,
            AppDbContext context, HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            var profileExists = await context.Profiles.AnyAsync(p => p.Id == request.ProfileId);
            if (!profileExists) return Results.BadRequest(new { error = "Profile no encontrado" });

            var userId = GetUserId(http);

            var trabajador = new Trabajador
            {
                ProfileId = request.ProfileId,
                Nombre = request.Nombre,
                TelefonoPersonal = request.TelefonoPersonal,
                EmpresaId = request.EmpresaId,
                SucursalId = request.SucursalId,
                Activo = true,
                FechaInicio = request.FechaInicio,
                CreadoPor = userId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            context.Trabajadores.Add(trabajador);
            await context.SaveChangesAsync();

            return Results.Created($"/api/v1/masters/trabajadores/{trabajador.Id}",
                new TrabajadorResponse(trabajador.Id, trabajador.ProfileId, trabajador.Nombre,
                    trabajador.TelefonoPersonal, trabajador.EmpresaId, trabajador.SucursalId,
                    trabajador.Activo, trabajador.FechaInicio, trabajador.FechaFin));
        });

        group.MapPut("/trabajadores/{id:guid}", async (Guid id, UpdateTrabajadorRequest request,
            AppDbContext context, HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            var t = await context.Trabajadores.FirstOrDefaultAsync(x => x.Id == id);
            if (t == null) return Results.NotFound();

            t.Nombre = request.Nombre;
            t.TelefonoPersonal = request.TelefonoPersonal;
            t.SucursalId = request.SucursalId;

            // Si se desactiva, registrar fecha de fin
            if (!request.Activo && t.Activo)
                t.FechaFin = DateOnly.FromDateTime(DateTime.UtcNow);

            t.Activo = request.Activo;
            await context.SaveChangesAsync();

            return Results.Ok(new TrabajadorResponse(t.Id, t.ProfileId, t.Nombre, t.TelefonoPersonal,
                t.EmpresaId, t.SucursalId, t.Activo, t.FechaInicio, t.FechaFin));
        });

        group.MapDelete("/trabajadores/{id:guid}", async (Guid id, AppDbContext context, HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            var t = await context.Trabajadores.FirstOrDefaultAsync(x => x.Id == id);
            if (t == null) return Results.NotFound();

            t.Activo = false;
            t.FechaFin = DateOnly.FromDateTime(DateTime.UtcNow);
            await context.SaveChangesAsync();

            return Results.Ok(new { deleted = true, id });
        });

        // ═══════════════════════════════════════════
        // PROFILES
        // ═══════════════════════════════════════════

        group.MapGet("/profiles", async (AppDbContext context, HttpContext http,
            [FromQuery] Guid? empresaId, [FromQuery] Guid? sucursalId,
            [FromQuery] string? rol, [FromQuery] bool? activo) =>
        {
            if (!IsAdminOrFinanzas(http)) return Results.Forbid();

            var query = context.Profiles.AsNoTracking();

            if (empresaId.HasValue) query = query.Where(p => p.EmpresaId == empresaId.Value);
            if (sucursalId.HasValue) query = query.Where(p => p.SucursalId == sucursalId.Value);
            if (!string.IsNullOrEmpty(rol)) query = query.Where(p => p.Rol == rol);
            if (activo.HasValue) query = query.Where(p => p.Activo == activo.Value);

            var items = await query
                .OrderBy(p => p.CreatedAt)
                .Select(p => new ProfileResponse(p.Id, p.PhoneNumber, p.Email, p.FullName,
                    p.EmpresaId, p.SucursalId, p.Rol, p.Activo, p.CreatedAt, p.LastLoginAt))
                .ToListAsync();

            return Results.Ok(items);
        });

        group.MapGet("/profiles/{id:guid}", async (Guid id, AppDbContext context, HttpContext http) =>
        {
            if (!IsAdminOrFinanzas(http)) return Results.Forbid();

            var profile = await context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            return profile is not null
                ? Results.Ok(new ProfileResponse(profile.Id, profile.PhoneNumber, profile.Email,
                    profile.FullName, profile.EmpresaId, profile.SucursalId, profile.Rol,
                    profile.Activo, profile.CreatedAt, profile.LastLoginAt))
                : Results.NotFound();
        });

        group.MapPost("/profiles", async (CreateProfileRequest request,
            AppDbContext context, IAuthService auth, HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            // Validar unicidad de phone y email si se proporcionan
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                var phoneExists = await context.Profiles
                    .AnyAsync(p => p.PhoneNumber == request.PhoneNumber);
                if (phoneExists)
                    return Results.BadRequest(new { error = "Ya existe un usuario con ese número de teléfono" });
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                var emailExists = await context.Profiles
                    .AnyAsync(p => p.Email == request.Email);
                if (emailExists)
                    return Results.BadRequest(new { error = "Ya existe un usuario con ese email" });
            }

            // Validar que haya al menos phone o email
            if (string.IsNullOrEmpty(request.PhoneNumber) && string.IsNullOrEmpty(request.Email))
                return Results.BadRequest(new { error = "Debe proporcionar teléfono o email" });

            var profile = new Profile
            {
                Id = Guid.NewGuid(),
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                PasswordHash = auth.HashPassword(request.Password),
                FullName = request.FullName,
                EmpresaId = request.EmpresaId,
                SucursalId = request.SucursalId,
                Rol = request.Rol,
                Activo = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            context.Profiles.Add(profile);
            await context.SaveChangesAsync();

            return Results.Created($"/api/v1/masters/profiles/{profile.Id}",
                new ProfileResponse(profile.Id, profile.PhoneNumber, profile.Email,
                    profile.FullName, profile.EmpresaId, profile.SucursalId, profile.Rol,
                    profile.Activo, profile.CreatedAt, profile.LastLoginAt));
        });

        group.MapPut("/profiles/{id:guid}", async (Guid id, UpdateProfileRequest request,
            AppDbContext context, HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            var profile = await context.Profiles.FirstOrDefaultAsync(p => p.Id == id);
            if (profile == null) return Results.NotFound();

            // Validar unicidad de phone (excluyendo el mismo registro)
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                var phoneExists = await context.Profiles
                    .AnyAsync(p => p.PhoneNumber == request.PhoneNumber && p.Id != id);
                if (phoneExists)
                    return Results.BadRequest(new { error = "Ya existe otro usuario con ese número de teléfono" });
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                var emailExists = await context.Profiles
                    .AnyAsync(p => p.Email == request.Email && p.Id != id);
                if (emailExists)
                    return Results.BadRequest(new { error = "Ya existe otro usuario con ese email" });
            }

            profile.PhoneNumber = request.PhoneNumber;
            profile.Email = request.Email;
            profile.FullName = request.FullName;
            profile.EmpresaId = request.EmpresaId;
            profile.SucursalId = request.SucursalId;
            profile.Rol = request.Rol;
            profile.Activo = request.Activo;

            await context.SaveChangesAsync();

            return Results.Ok(new ProfileResponse(profile.Id, profile.PhoneNumber, profile.Email,
                profile.FullName, profile.EmpresaId, profile.SucursalId, profile.Rol,
                profile.Activo, profile.CreatedAt, profile.LastLoginAt));
        });

        group.MapDelete("/profiles/{id:guid}", async (Guid id, AppDbContext context, HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            var profile = await context.Profiles.FirstOrDefaultAsync(p => p.Id == id);
            if (profile == null) return Results.NotFound();

            profile.Activo = false;
            await context.SaveChangesAsync();
            return Results.Ok(new { deleted = true, id });
        });

        group.MapPut("/profiles/{id:guid}/password", async (Guid id,
            ResetProfilePasswordRequest request, AppDbContext context, IAuthService auth,
            HttpContext http) =>
        {
            if (!IsAdmin(http)) return Results.Forbid();

            var profile = await context.Profiles.FirstOrDefaultAsync(p => p.Id == id);
            if (profile == null) return Results.NotFound();

            if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < 8)
                return Results.BadRequest(new { error = "La contraseña debe tener al menos 8 caracteres" });

            profile.PasswordHash = auth.HashPassword(request.NewPassword);
            await context.SaveChangesAsync();

            return Results.Ok(new { success = true, message = "Contraseña actualizada" });
        });
    }

    // Helpers
    private static bool IsAdmin(HttpContext http) => http.User.FindFirst(ClaimTypes.Role)?.Value is "admin";
    private static Guid GetUserId(HttpContext http)
        => Guid.Parse(http.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    private static bool IsAdminOrFinanzas(HttpContext http) => http.User.FindFirst(ClaimTypes.Role)?.Value is "admin" or "finanzas";
}