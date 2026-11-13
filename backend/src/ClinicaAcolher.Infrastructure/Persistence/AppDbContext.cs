using ClinicaAcolher.Domain.Patients;
using ClinicaAcolher.Domain.Professionals;
using ClinicaAcolher.Domain.Relationships;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAcolher.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Professional> Professionals => Set<Professional>();
    public DbSet<TherapeuticRelationship> TherapeuticRelationships => Set<TherapeuticRelationship>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
