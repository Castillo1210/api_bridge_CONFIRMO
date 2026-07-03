namespace Confirmo.Api.Models.DTOs;

public record LoginRequest(string PhoneNumber, string Password, string? FcmToken = null);

public record LoginResponse(string AccessToken, string RefreshToken, int ExpiresInSeconds, UserInfo User);

public record UserInfo(Guid Id, string PhoneNumber, string FullName, Guid EmpresaId, Guid? SucursalId, string? FcmToken);

public record RefreshRequest(string RefreshToken);

public record RefreshResponse(string AccessToken, int ExpiresInSeconds);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record ChangePasswordResponse(bool Success, string Message);

public record UpdateFcmTokenRequest(string Token);