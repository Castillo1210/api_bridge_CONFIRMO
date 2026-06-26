using System.Security.Claims;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Models.Entities;

namespace Confirmo.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<RefreshResponse?> RefreshAsync(string refreshToken);
    string GenerateAccessToken(Profile user);
    string GenerateRefreshToken();
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    ClaimsPrincipal? ValidateToken(string token);
    Task<ChangePasswordResponse> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
}