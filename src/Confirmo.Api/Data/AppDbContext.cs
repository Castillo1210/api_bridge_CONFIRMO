using Confirmo.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Confirmo.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Deposito> Depositos => Set<Deposito>();
    public DbSet<Banco> Bancos => Set<Banco>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<DepositMessage> DepositMessages => Set<DepositMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}