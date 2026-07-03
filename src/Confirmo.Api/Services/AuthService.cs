using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Confirmo.Api.Data;
using Confirmo.Api.Models.DTOs;
using Confirmo.Api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Confirmo.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext context, IConfiguration config, ILogger<AuthService> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    public async Task<ChangePasswordResponse> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _context.Profiles.FirstOrDefaultAsync(p => p.Id == userId && p.Activo);
        if (user == null) return new ChangePasswordResponse(false, "Usuario no encontrado");

        // 🔍 LOGS DE DIAGNÓSTICO (Añade estas líneas)
        _logger.LogInformation("--- DIAGNÓSTICO DE CONTRASEÑA ---");
        _logger.LogInformation("Password recibida de Postman: '{CurrentPassword}' (Largo: {Length})", currentPassword, currentPassword?.Length);
        _logger.LogInformation("Hash leído de la Base de Datos: '{PasswordHash}' (Largo: {HashLength})", user.PasswordHash, user.PasswordHash?.Length);

        bool esValido = VerifyPassword(currentPassword, user.PasswordHash);

        _logger.LogInformation("El Password es correcto: {EsValido}", esValido);

        if (!VerifyPassword(currentPassword, user.PasswordHash))
        {
            return new ChangePasswordResponse(false, "Contraseña actual incorrecta");
        }

        if (currentPassword == newPassword)
        {
            return new ChangePasswordResponse(false, "La nueva contraseña debe ser diferente a la actual");
        }

        if (newPassword.Length < 8)
        {
            return new ChangePasswordResponse(false, "La contraseña debe tener al menos 8 caracteres");
        }

        user.PasswordHash = HashPassword(newPassword);
        await _context.SaveChangesAsync();

        return new ChangePasswordResponse(true, "Contraseña actualizado correctamente");
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Profiles.FirstOrDefaultAsync(p => p.PhoneNumber == request.PhoneNumber && p.Activo);

        if (user == null)
        {
            _logger.LogWarning("Login fallido para número: {PhoneNumber}", request.PhoneNumber);
            return null;
        }

        var hashNativo = HashPassword(request.Password);
        _logger.LogInformation("El Has nativo es: {Hash}", hashNativo);

        // 2. 🚨 ¡LA LÍNEA FALTANTE! Validamos la contraseña contra el Hash real
        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Contraseña incorrecta para número: {PhoneNumber}", request.PhoneNumber);
            return null; // Si no coincide, rechaza el login de inmediato
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        _context.Entry(user).Property(x => x.LastLoginAt).IsModified = true;

        _context.Entry(user).Property(x => x.PasswordHash).IsModified = false;

        await _context.SaveChangesAsync();

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        return new LoginResponse(
            accessToken,
            refreshToken,
            _config.GetValue<int>("Jwt:AccessTokenHours") * 3600,
            new UserInfo(user.Id, user.PhoneNumber, user.FullName, user.EmpresaId, user.SucursalId, user.FcmToken)
        );
    }

    public async Task<RefreshResponse?> RefreshAsync(string refreshToken)
    {
        var principal = ValidateToken(refreshToken);
        if (principal == null) return null;

        var userId = Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var user = await _context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId && p.Activo);
        if (user == null) return null;

        var newAccessToken = GenerateAccessToken(user);
        return new RefreshResponse(newAccessToken, _config.GetValue<int>("Jwt:AccessTokenHours") * 3600);
    }

    public string GenerateAccessToken(Profile user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new []
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.HomePhone, user.PhoneNumber),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("empresaId(empresa_id)", user.EmpresaId.ToString()),
            new Claim(ClaimTypes.Role, user.Rol)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "confirmo-api",
            audience: _config["Jwt:Audience"] ?? "confirmo-app",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_config.GetValue<int>("Jwt:AccessTokenHours")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!);

        try
        {
            return handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"] ?? "confirmo-api",
                ValidAudience = _config["Jwt:Audience"] ?? "confirmo-app",
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            }, out _);
        }
        catch
        {
            return null;
        }
    }
}