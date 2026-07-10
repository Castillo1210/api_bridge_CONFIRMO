using System.Text.Json;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Models.Entities;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;

namespace Confirmo.Api.Services;

public interface IFCMNotificationService
{
    Task SendNotificationAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null);
    Task SendDepositConfirmedAsync(string fcmToken, DepositConfirmedNotification notification);
    Task SendDepositRejectedAsync(string fcmToken, string reason);
    Task SendProcessingAsync(string fcmToken, string message);
}

public class FCMNotificationService : IFCMNotificationService
{
    private readonly ILogger<FCMNotificationService> _logger;
    private readonly bool _initialzed;

    public FCMNotificationService(IConfiguration config, ILogger<FCMNotificationService> logger)
    {
        _logger = logger;

        try
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                var projectId = config["Firebase:ProjectId"];

                if (!string.IsNullOrEmpty(projectId))
                {
                    FirebaseApp.Create(new AppOptions 
                    {
                        Credential = Google.Apis.Auth.OAuth2.GoogleCredential.GetApplicationDefault(), 
                        ProjectId = projectId 
                    });
                }
                else
                {
                    FirebaseApp.Create();
                }
            }

            _initialzed = true;
            _logger.LogInformation("Firebase Admin SDK inicializado correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar Firebase - FCM deshabilitado");
            _initialzed = false;
        }
    }

    public async Task SendNotificationAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null)
    {
        if (!_initialzed || string.IsNullOrEmpty(fcmToken))
        {
            _logger.LogWarning("FCM no inicializado o token vacío");
            return;
        }

        try
        {
            var message = new Message
            {
                Token = fcmToken,
                Notification = new Notification { Title = title, Body = body },
                Data = data ?? new Dictionary<string, string>(),
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Icon = "ic_notification",
                        Color = "#197602",
                        ChannelId = "deposits_channel"
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps { Alert = new ApsAlert { Title = title, Body = body } }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("FCM enviado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando FCM a {Token}", fcmToken[..10]);
        }
    }

    public async Task SendDepositConfirmedAsync(string fcmToken, DepositConfirmedNotification notification)
    {
        var title = "🎉 Depósito Confirmado";
        var body = $"{notification.Empresa} - {notification.Importe} {notification.Moneda}";

        var data = new Dictionary<string, string>
        {
            ["type"] = "deposit_confirmed",
            ["depositId"] = notification.DepositId.ToString(),
            ["referenceNumber"] = notification.ReferenceNumber,
            ["empresa"] = notification.Empresa,
            ["sucursal"] = notification.Sucursal,
            ["banco"] = notification.Banco,
            ["anexo"] = notification.Anexo,
            ["fechaDeposito"] = notification.FechaDeposito.ToString("yyyy-MM-dd"),
            ["numeroOperacion"] = notification.NumeroOperacion,
            ["importe"] = notification.Importe,
            ["moneda"] = notification.Moneda
        };

        await SendNotificationAsync(fcmToken, title, body, data);
    }

    public async Task SendDepositRejectedAsync(string fcmToken, string reason)
    {
        var title = "✖️ Depósito Rechazado";
        var body = $"Tu depósito ha sido rechazado: {reason}";

        await SendNotificationAsync(fcmToken, title, body);
    }

    public async Task SendProcessingAsync(string fcmToken, string message)
    {
        await SendNotificationAsync(fcmToken, "⏳ Procesando", message, new Dictionary<string, string>
        {
            ["type"] = "deposit_processing"
        });
    }
}

public static class FCMNotificationServiceExtensions
{
    public static IServiceCollection AddFCMNotifications(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IFCMNotificationService, FCMNotificationService>();
        return services;
    }
}