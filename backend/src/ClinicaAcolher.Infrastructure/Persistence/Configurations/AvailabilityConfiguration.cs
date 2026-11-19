using ClinicaAcolher.Domain.Availability;
using ClinicaAcolher.Domain.Professionals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaAcolher.Infrastructure.Persistence.Configurations;

internal sealed class ProfessionalAvailabilityConfiguration : IEntityTypeConfiguration<ProfessionalAvailability>
{
    public void Configure(EntityTypeBuilder<ProfessionalAvailability> builder)
    {
        builder.ToTable("professional_availabilities");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.TimeZoneId).HasMaxLength(64).IsRequired();
        builder.Ignore(a => a.DefaultSession);

        builder.HasOne<Professional>().WithMany().HasForeignKey(a => a.ProfessionalId).OnDelete(DeleteBehavior.Restrict);

        // Cada profissional tem uma única disponibilidade
        builder.HasIndex(a => a.ProfessionalId).IsUnique();

        // Os períodos são value objects que pertencem à disponibilidade
        builder.OwnsMany(a => a.Windows, window =>
        {
            window.ToTable("availability_windows");
            window.WithOwner().HasForeignKey("AvailabilityId");
            window.Property<int>("Id");
            window.HasKey("Id");
            window.Property(w => w.Weekday).HasConversion<string>().HasMaxLength(10);
            window.Ignore(w => w.Length);
        });
    }
}

internal sealed class AvailabilityBlockConfiguration : IEntityTypeConfiguration<AvailabilityBlock>
{
    public void Configure(EntityTypeBuilder<AvailabilityBlock> builder)
    {
        builder.ToTable("availability_blocks", t =>
            t.HasCheckConstraint("ck_availability_blocks_period", "\"EndsAt\" > \"StartsAt\""));

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();
        builder.Property(b => b.Reason).HasMaxLength(AvailabilityBlock.MaxReasonLength);

        builder.HasOne<Professional>().WithMany().HasForeignKey(b => b.ProfessionalId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(b => new { b.ProfessionalId, b.StartsAt });
    }
}