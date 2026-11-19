using ClinicaAcolher.Domain.Appointments;
using ClinicaAcolher.Domain.Patients;
using ClinicaAcolher.Domain.Professionals;
using ClinicaAcolher.Domain.Relationships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaAcolher.Infrastructure.Persistence.Configurations;

internal sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments", t =>
            t.HasCheckConstraint("ck_appointments_period", "\"EndsAt\" > \"StartsAt\""));

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.CancelledBy).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.CancellationReason).HasMaxLength(500);

        builder.Ignore(a => a.Duration);
        builder.Ignore(a => a.CancellationNotice);

        builder.HasOne<TherapeuticRelationship>().WithMany().HasForeignKey(a => a.RelationshipId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Patient>().WithMany().HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Professional>().WithMany().HasForeignKey(a => a.ProfessionalId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Appointment>().WithMany().HasForeignKey(a => a.RescheduledToId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AppointmentSeries>().WithMany().HasForeignKey(a => a.SeriesId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.ProfessionalId, a.StartsAt });

        // RN-AGD-05: uma série nunca gera duas sessões no mesmo horário
        builder.HasIndex(a => new { a.SeriesId, a.StartsAt }).IsUnique();
    }
}
