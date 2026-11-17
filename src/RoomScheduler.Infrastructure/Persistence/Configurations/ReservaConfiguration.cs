using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomScheduler.Domain.Entities;

namespace RoomScheduler.Infrastructure.Persistence.Configurations;

public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Subject)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(r => r.StartsAt).IsRequired();
        builder.Property(r => r.EndsAt).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasOne(r => r.Professor)
               .WithMany()
               .HasForeignKey(r => r.ProfessorId)
               .OnDelete(DeleteBehavior.Restrict);

        // Index for fast daily conflict-check queries.
        builder.HasIndex(r => new { r.SalaId, r.StartsAt });
    }
}
