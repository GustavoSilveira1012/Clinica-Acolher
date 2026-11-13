using ClinicaAcolher.Domain.Appointments;
using ClinicaAcolher.Domain.Attendance;
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
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AttendancePolicy> AttendancePolicies => Set<AttendancePolicy>();
    public DbSet<AttendancePolicyAcceptance> AttendancePolicyAcceptances => Set<AttendancePolicyAcceptance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("btree_gist");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
