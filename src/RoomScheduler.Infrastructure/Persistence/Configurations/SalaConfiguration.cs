using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomScheduler.Domain.Entities;

namespace RoomScheduler.Infrastructure.Persistence.Configurations;

public class SalaConfiguration : IEntityTypeConfiguration<Sala>
{
    public void Configure(EntityTypeBuilder<Sala> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(s => s.Capacity)
               .IsRequired();

        builder.Property(s => s.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        // Shadow the private _reservas backing field so EF can populate it.
        builder.HasMany(s => s.Reservas)
               .WithOne(r => r.Sala)
               .HasForeignKey(r => r.SalaId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
