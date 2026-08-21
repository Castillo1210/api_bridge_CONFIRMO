using System.Text.Json;

namespace Confirmo.Api.Services;

public static class DepositBusinessRules
{
    private static readonly TimeZoneInfo ZonaPeru = ObtenerZonaPeru();

    private static TimeZoneInfo ObtenerZonaPeru()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Lima");
        }
        catch (TimeZoneNotFoundException)
        {
            // Windows usa un ID distinto al de Linux/IANA
            return TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        }
    }

    public static DateOnly HoyPeru() => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaPeru));

    public static (string? accion, double? confianza) LeerVerificacionFecha(object? datosOcr)
    {
        if (datosOcr is null) return (null, null);

        JsonElement root;
        try
        {
            root = datosOcr switch
            {
                JsonElement je => je,
                JsonDocument jd => jd.RootElement,
                string s => JsonDocument.Parse(s).RootElement,
                _ => JsonDocument.Parse(JsonSerializer.Serialize(datosOcr)).RootElement
            };
        }
        catch
        {
            return (null, null);
        }

        if (!root.TryGetProperty("verificacion", out var verificacion)) return (null, null);
        if (!verificacion.TryGetProperty("fecha_deposito", out var fecha)) return (null, null);

        string? accion = fecha.TryGetProperty("accion", out var accionE1) ? accionE1.GetString() : null;

        double? confianza = null;
        if (root.TryGetProperty("llama_field_confidence", out var confMap) && confMap.TryGetProperty("fecha_deposito", out var confEl) && confEl.TryGetDouble(out var confVal))
        {
            confianza = confVal;
        }

        return (accion, confianza);
    }

    public static bool PuedeAutoclasificarAntiguo(string? accion, double? confianza, double umbral = 0.85)
    {
        if (accion == "ninguna") return true;
        if (accion == "auto_corregido" && confianza is not null && confianza >= umbral) return true;
        return false;
    }
}