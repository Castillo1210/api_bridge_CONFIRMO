using Confirmo.Api.Data;
using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Confirmo.Api.Services;

public class AvisoDispatchService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AvisoDispatchService> _logger;
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);

    public AvisoDispatchService(IServiceScopeFactory scopeFactory, ILogger<AvisoDispatchService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarAvisosPendientesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando avisos pendientes");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcesarAvisosPendientesAsync(CancellationToken cts)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var zavu = scope.ServiceProvider.GetRequiredService<IZavuClient>();
        var fcm = scope.ServiceProvider.GetRequiredService<IFCMNotificationService>();

        var ahora = DateTimeOffset.UtcNow;
        var pendientes = await context.Avisos
            .Where(a => a.Activo && a.ProximaEjecucion != null && a.ProximaEjecucion <= ahora)
            .ToListAsync(cts);

        foreach (var aviso in pendientes)
        {
            await DespacharAvisoAsync(aviso, context, zavu, fcm, cts);
        }
    }

    private async Task DespacharAvisoAsync(Aviso aviso, AppDbContext context, IZavuClient zavu, IFCMNotificationService fcm, CancellationToken cts)
    {
        var destinatarios = await context.Profiles
            .Where(p => p.Activo && aviso.RolesDestino.Contains(p.Rol))
            .ToListAsync(cts);

        var runToken = Guid.NewGuid().ToString("N")[..8];

        _logger.LogInformation("Despachando aviso {AvisoId} a {Count} destinatarios", aviso.Id, destinatarios.Count);

        foreach (var perfil in destinatarios)
        {
            if (aviso.EnviarApp && !string.IsNullOrEmpty(perfil.FcmToken))
            {
                try
                {
                    await fcm.SendNotificationAsync(perfil.FcmToken, aviso.Titulo, aviso.MensajeTexto, new Dictionary<string, string> { ["type"] = "aviso", ["avisoId"] = aviso.Id.ToString() });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error enviando push de aviso");
                }
            }

            if (aviso.EnviarWhatsapp && !string.IsNullOrEmpty(perfil.PhoneNumber))
            {
                var result = await zavu.SendAsync(perfil.PhoneNumber!, aviso.MensajeTexto, "whatapp", idempotencyKey: $"aviso-{aviso.Id}-{runToken}-{perfil.Id}-whatsapp", cts: cts);

                context.EnvioAvisoLogs.Add(new EnvioAvisoLog
                {
                    AvisoId = aviso.Id,
                    ProfileId = perfil.Id,
                    Canal = "whatsapp",
                    Estado = result.Success ? "enviado" : "error",
                    ZavuMessageId = result.MessageId,
                    Error = result.Error
                });
            }

            if (aviso.EnviarEmail && !string.IsNullOrEmpty(perfil.Email))
            {
                var result = await zavu.SendAsync(
                    perfil.Email!, aviso.MensajeTexto, "email",
                    idempotencyKey: $"aviso-{aviso.Id}-{runToken}-{perfil.Id}-email",
                    subject: aviso.AsuntoEmail ?? aviso.Titulo, cts: cts
                );

                context.EnvioAvisoLogs.Add(new EnvioAvisoLog
                {
                    AvisoId = aviso.Id,
                    ProfileId = perfil.Id,
                    Canal = "email",
                    Estado = result.Success ? "enviado" : "error",
                    ZavuMessageId = result.MessageId,
                    Error = result.Error
                });
            }
        }

        aviso.UltimaEjecucion = DateTimeOffset.UtcNow;

        if (aviso.EsRecurrente)
        {
            aviso.ProximaEjecucion = CalcularProximaEjecucion(aviso, DateTimeOffset.UtcNow);
        }
        else
        {
            aviso.Estado = "enviado";
            aviso.ProximaEjecucion = null;
        }

        await context.SaveChangesAsync();
    }

    private static DateTimeOffset CalcularProximaEjecucion(Aviso aviso, DateTimeOffset ahora)
    {
        var hora = aviso.HoraEjecucion ?? TimeSpan.Zero;

        switch (aviso.Frecuencia)
        {
            case "semanal":
            {
                var diaObjetivo = aviso.DiaSemana ?? (int)ahora.DayOfWeek;
                var candidata = ahora.Date.Add(hora);
                var diasHasta = ((diaObjetivo - (int)candidata.DayOfWeek) + 7) % 7;
                candidata = candidata.AddDays(diasHasta);
                if (candidata <= ahora) candidata = candidata.AddDays(7);
                return new DateTimeOffset(candidata, TimeSpan.Zero);
            }
            case "mensual":
            {
                var diaMes = aviso.DiaMes ?? ahora.Day;
                DateTime ArmarFecha(int year, int month) => new DateTime(year, month, 1)
                    .AddDays(Math.Min(diaMes, DateTime.DaysInMonth(year, month)) - 1)
                    .Add(hora);

                var candidata = ArmarFecha(ahora.Year, ahora.Month);
                if (candidata <= ahora)
                {
                    var siguiente = ahora.AddMonths(1);
                    candidata = ArmarFecha(siguiente.Year, siguiente.Month);
                }
                return new DateTimeOffset(candidata, TimeSpan.Zero);
            }
            default:
            {
                var candidata = ahora.Date.Add(hora);
                if (candidata <= ahora) candidata = candidata.AddDays(1);
                return new DateTimeOffset(candidata, TimeSpan.Zero);
            }
        }
    }
}