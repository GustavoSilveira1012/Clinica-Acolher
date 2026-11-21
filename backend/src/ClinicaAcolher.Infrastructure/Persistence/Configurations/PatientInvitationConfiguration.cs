using ClinicaAcolher.Domain.PatientAccess;
using ClinicaAcolher.Domain.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaAcolher.Infrastructure.Persistence.Configurations;

internal sealed class PatientInvitationConfiguration : IEntityTypeConfiguration<PatientInvitation>
{
    public void Configure(EntityTypeBuilder<PatientInvitation> builder)
    {
        builder.ToTable("patient_invitations");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();
        builder.Property(i => i.TokenHash).IsRequired();

        builder.HasOne<Patient>().WithMany().HasForeignKey(i => i.PatientId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.TokenHash).IsUnique();
        builder.HasIndex(i => i.PatientId);
    }
}