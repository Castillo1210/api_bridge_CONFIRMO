using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Confirmo.Api.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connStr = configuration.GetConnectionString("Default")
                      ?? BuildConnectionString(configuration);

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connStr,
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", "public"));

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string BuildConnectionString(IConfiguration config)
    {
        var db = config.GetSection("Database");
        return $"Host={db["Host"]};Port={db["Port"]};Database={db["Name"]};" +
               $"Username={db["User"]};Password={db["Password"]};" +
               $"Pooling=true;MinPoolSize=2;MaxPoolSize=20";
    }
}