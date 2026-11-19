using ClinicaAcolher.Domain.Appointments;
using ClinicaAcolher.Domain.Patients;
using ClinicaAcolher.Domain.Professionals;
using ClinicaAcolher.Domain.Relationships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaAcolher.Infrastructure.Persistence.Configurations;

internal sealed class AppointmentSeriesConfiguration : IEntityTypeConfiguration<AppointmentSeries>
{
    public void Configure(EntityTypeBuilder<AppointmentSeries> builder)
    {
        builder.ToTable("appointment_series");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Weekday).HasConversion<string>().HasMaxLength(10);
        builder.Property(s => s.TimeZoneId).HasMaxLength(64).IsRequired();

        builder.Ignore(s => s.IsActive);
        builder.Ignore(s => s.Duration);

        builder.HasOne<TherapeuticRelationship>().WithMany().HasForeignKey(s => s.RelationshipId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Patient>().WithMany().HasForeignKey(s => s.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Professional>().WithMany().HasForeignKey(s => s.ProfessionalId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.GeneratedUntil);
    }
}