using Confirmo.Api.Data;
using Confirmo.Api.Services;
using Confirmo.Api.Hubs;
using Confirmo.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Confirmo.Api.Endpoints;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Configuración
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// DbContext
var connStr = builder.Configuration.GetConnectionString("Default") 
    ?? BuildConnectionString(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(o => 
    o.UseNpgsql(connStr, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", "public")));

// Auth
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret requerido");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"].FirstOrDefault();
                if (!string.IsNullOrEmpty(token)) ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Servicios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddHttpClient<IPythonWorkerClient, PythonWorkerClient>();
builder.Services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();
builder.Services.AddScoped<IChatService, ChatService>();

// SignalR
builder.Services.AddSignalR(o => o.EnableDetailedErrors = builder.Configuration.GetValue<bool>("SignalR:EnableDetailedErrors"));

// HttpClient para Python Worker
builder.Services.AddHttpClient<IPythonWorkerClient, PythonWorkerClient>(c => 
    c.BaseAddress = new Uri(builder.Configuration["PythonWorker:BaseUrl"]!));

// Controllers/Endpoints
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapAuthEndpoints();
app.MapDepositEndpoints();
app.MapInternalEndpoints();
app.MapChatEndpoints();

// SignalR Hub
app.MapHub<DepositHub>("/hubs/deposits");

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }));

// Migraciones automáticas en desarrollo
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

static string BuildConnectionString(IConfiguration config)
{
    var db = config.GetSection("Database");
    return $"Host={db["Host"]};Port={db["Port"]};Database={db["Name"]};Username={db["User"]};Password={db["Password"]};Pooling=true;MinPoolSize=2;MaxPoolSize=20";
}

public record JwtOptions
{
    public string Secret { get; init; } = "";
    public int AccessTokenHours { get; init; } = 8;
    public int RefreshTokenDays { get; init; } = 30;
    public string Issuer { get; init; } = "confirmo-api";
    public string Audience { get; init; } = "confirmo-app";
}
