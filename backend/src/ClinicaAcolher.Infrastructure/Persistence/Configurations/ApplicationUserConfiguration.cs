using ClinicaAcolher.Domain.Patients;
using ClinicaAcolher.Domain.Professionals;
using ClinicaAcolher.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaAcolher.Infrastructure.Persistence.Configurations;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasOne<Professional>().WithMany().HasForeignKey(u => u.ProfessionalId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Patient>().WithMany().HasForeignKey(u => u.PatientId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.ProfessionalId).IsUnique();
        builder.HasIndex(u => u.PatientId).IsUnique();
    }
}