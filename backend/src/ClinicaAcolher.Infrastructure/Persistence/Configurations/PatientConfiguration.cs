using ClinicaAcolher.Domain.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaAcolher.Infrastructure.Persistence.Configurations;

internal sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.FullName).HasMaxLength(Patient.NameMaxLength).IsRequired();
        builder.Property(p => p.SocialName).HasMaxLength(Patient.NameMaxLength);
        builder.Property(p => p.Email).HasMaxLength(254);
        builder.Property(p => p.Phone).HasMaxLength(30);
        builder.Property(p => p.DeactivationReason).HasMaxLength(500);

        builder.Ignore(p => p.DisplayName);
    }
}
