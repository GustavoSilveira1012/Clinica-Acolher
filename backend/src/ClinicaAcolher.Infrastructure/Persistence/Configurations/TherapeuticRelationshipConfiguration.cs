using ClinicaAcolher.Domain.Patients;
using ClinicaAcolher.Domain.Professionals;
using ClinicaAcolher.Domain.Relationships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaAcolher.Infrastructure.Persistence.Configurations;

internal sealed class TherapeuticRelationshipConfiguration : IEntityTypeConfiguration<TherapeuticRelationship>
{
    public void Configure(EntityTypeBuilder<TherapeuticRelationship> builder)
    {
        builder.ToTable("therapeutic_relationships");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.EndReason).HasMaxLength(500);
        builder.Ignore(r =>r.AllowsScheduling);

        builder.HasOne<Patient>().WithMany().HasForeignKey(r => r.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Professional>().WithMany().HasForeignKey(r => r.ProfessionalId).OnDelete(DeleteBehavior.Restrict);

        //RN-VIN-02: no máximo um vínculo não encerrado por par paciente-profissional
        builder.HasIndex(r => new {r.PatientId, r.ProfessionalId})
        .IsUnique()
        .HasFilter("\"Status\" <> 'Ended'");
    }
}