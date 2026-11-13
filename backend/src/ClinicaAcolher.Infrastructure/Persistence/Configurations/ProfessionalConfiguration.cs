using ClinicaAcolher.Domain.Professionals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaAcolher.Infrastructure.Persistence.Configurations;

internal sealed class ProfessionalConfiguration : IEntityTypeConfiguration<Professional>
{
    public void Configure(EntityTypeBuilder<Professional> builder)
    {
        builder.ToTable("professionals");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.FullName).HasMaxLength(Professional.NameMaxLength).IsRequired();
        builder.Property(p => p.ProfessionalName).HasMaxLength(Professional.NameMaxLength).IsRequired();
        builder.Property(p => p.Email).HasMaxLength(254).IsRequired();
        builder.Property(p => p.RegistrationNumber).HasMaxLength(30);

        builder.Property(p => p.Category).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Verification).HasConversion<string>().HasMaxLength(20);

        builder.Ignore(p => p.CanAttend);

        builder.HasIndex(p => p.Email).IsUnique();
    }
}