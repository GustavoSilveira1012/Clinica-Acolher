using ClinicaAcolher.Domain.Appointments;
using ClinicaAcolher.Domain.ClinicalNotes;
using ClinicaAcolher.Domain.Patients;
using ClinicaAcolher.Domain.Professionals;
using ClinicaAcolher.Domain.Relationships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaAcolher.Infrastructure.Persistence.Configurations;

internal sealed class ClinicalNoteConfiguration : IEntityTypeConfiguration<ClinicalNote>
{
    public void Configure(EntityTypeBuilder<ClinicalNote> builder)
    {
        builder.ToTable("clinical_notes");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();
        builder.Property(n => n.Status).HasConversion<string>().HasMaxLength(20);

        builder.Ignore(n => n.NextRevisionNumber);
        builder.Ignore(n => n.NextAddendumNumber);
        builder.Ignore(n => n.CurrentRevision);

        builder.HasOne<TherapeuticRelationship>().WithMany().HasForeignKey(n => n.RelationshipId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Patient>().WithMany().HasForeignKey(n => n.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Professional>().WithMany().HasForeignKey(n => n.ProfessionalId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Appointment>().WithMany().HasForeignKey(n => n.AppointmentId).OnDelete(DeleteBehavior.Restrict);

        // As partes do agregado: o EF usa os campos privados _revisions e _addenda
        builder.HasMany(n => n.Revisions).WithOne().HasForeignKey(r => r.NoteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(n => n.Addenda).WithOne().HasForeignKey(a => a.NoteId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => new { n.ProfessionalId, n.PatientId });
    }
}

internal sealed class ClinicalNoteRevisionConfiguration : IEntityTypeConfiguration<ClinicalNoteRevision>
{
    public void Configure(EntityTypeBuilder<ClinicalNoteRevision> builder)
    {
        builder.ToTable("clinical_note_revisions");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.EncryptedContent).IsRequired();
        builder.HasIndex(r => new { r.NoteId, r.Number }).IsUnique();
    }
}

internal sealed class ClinicalNoteAddendumConfiguration : IEntityTypeConfiguration<ClinicalNoteAddendum>
{
    public void Configure(EntityTypeBuilder<ClinicalNoteAddendum> builder)
    {
        builder.ToTable("clinical_note_addenda");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.EncryptedContent).IsRequired();
        builder.HasIndex(a => new { a.NoteId, a.Number }).IsUnique();
    }
}