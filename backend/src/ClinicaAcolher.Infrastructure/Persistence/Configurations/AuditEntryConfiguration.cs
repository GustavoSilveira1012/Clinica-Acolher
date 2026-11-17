using ClinicaAcolher.Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaAcolher.Infrastructure.Persistence.Configurations;

internal sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("audit_log");

        builder.HasKey(e => e.Sequence);
        builder.Property(e => e.Sequence).ValueGeneratedNever();

        builder.Property(e => e.Action).HasMaxLength(64).IsRequired();
        builder.Property(e => e.ResourceType).HasMaxLength(64).IsRequired();
        builder.Property(e => e.PreviousHash).IsRequired();
        builder.Property(e => e.Hash).IsRequired();

        // Para responder "quem acessou os dados desta paciente?" (RN-LGPD-04)
        builder.HasIndex(e => e.PatientId);
    }
}