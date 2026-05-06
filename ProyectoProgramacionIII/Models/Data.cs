using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ProyectoProgramacionIII.Models;

namespace ProyectoProgramacionIII.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Archivo> Archivos { get; set; }  // ← Cambiado de "Archivios" a "Archivos"

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Archivo>()
            .HasIndex(a => a.NombreOriginal);

        // Configuración para BYTEA (optimización)
        modelBuilder.Entity<Archivo>()
            .Property(a => a.Contenido)
            .HasColumnType("bytea");
    }
}
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Usar la cadena de conexión DIRECTAMENTE (sin appsettings.json)
        var connectionString = "Host=ep-dry-voice-aqdx5907.c-8.us-east-1.aws.neon.tech; Database=neondb; Username=neondb_owner; Password=npg_YW0JDOzkAq9v; SSL Mode=Require; Trust Server Certificate=true;";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString,
            npgsqlOptions => {
                npgsqlOptions.CommandTimeout(60);
                npgsqlOptions.EnableRetryOnFailure(3);
            });

        return new AppDbContext(optionsBuilder.Options);
    }
}