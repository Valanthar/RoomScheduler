using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomScheduler.Domain.Entities;

namespace RoomScheduler.Infrastructure.Persistence.Configurations;

public class ProfessorConfiguration : IEntityTypeConfiguration<Professor>
{
    public void Configure(EntityTypeBuilder<Professor> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(p => p.Email)
               .IsRequired()
               .HasMaxLength(255);

        builder.HasIndex(p => p.Email)
               .IsUnique();

        builder.Property(p => p.PasswordHash)
               .IsRequired();

        builder.Property(p => p.Role)
               .IsRequired();

        builder.Property(p => p.CreatedAt)
               .IsRequired();
    }
}
