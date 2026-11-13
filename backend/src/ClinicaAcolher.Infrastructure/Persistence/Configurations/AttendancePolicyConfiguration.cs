using ClinicaAcolher.Domain.Attendance;
using ClinicaAcolher.Domain.Patients;
using ClinicaAcolher.Domain.Professionals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaAcolher.Infrastructure.Persistence.Configurations;

internal sealed class AttendancePolicyConfiguration : IEntityTypeConfiguration<AttendancePolicy>
{
    public void Configure(EntityTypeBuilder<AttendancePolicy> builder)
    {
        builder.ToTable("attendance_policies", t =>
        {
            t.HasCheckConstraint("ck_attendance_policies_late_percent", "\"LateCancellationChargePercent\" BETWEEN 0 AND 100");
            t.HasCheckConstraint("ck_attendance_policies_no_show_percent", "\"NoShowChargePercent\" BETWEEN 0 AND 100");
        });

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Ignore(p => p.MinimumNotice);

        builder.HasOne<Professional>().WithMany().HasForeignKey(p => p.ProfessionalId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.ProfessionalId, p.Version }).IsUnique();
    }
}

internal sealed class AttendancePolicyAcceptanceConfiguration : IEntityTypeConfiguration<AttendancePolicyAcceptance>
{
    public void Configure(EntityTypeBuilder<AttendancePolicyAcceptance> builder)
    {
        builder.ToTable("attendance_policy_acceptances");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.HasOne<Patient>().WithMany().HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Professional>().WithMany().HasForeignKey(a => a.ProfessionalId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AttendancePolicy>().WithMany().HasForeignKey(a => a.PolicyId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.PatientId, a.PolicyId }).IsUnique();
        builder.HasIndex(a => new { a.ProfessionalId, a.PatientId });
    }
}