using Microsoft.EntityFrameworkCore;
using RoomScheduler.Domain.Entities;

namespace RoomScheduler.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Professor> Professors { get; set; } = null!;
    public DbSet<Sala>      Salas      { get; set; } = null!;
    public DbSet<Reserva>   Reservas   { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all IEntityTypeConfiguration<T> classes in this assembly automatically.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
