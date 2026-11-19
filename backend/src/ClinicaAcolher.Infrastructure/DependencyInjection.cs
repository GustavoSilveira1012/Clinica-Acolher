using ClinicaAcolher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ClinicaAcolher.Application.Patients;
using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Application.Professionals;
using ClinicaAcolher.Application.Relationships;
using ClinicaAcolher.Application.Appointments;
using ClinicaAcolher.Application.Attendance;
using ClinicaAcolher.Application.Availability;
using ClinicaAcolher.Application.ClinicalNotes;
using ClinicaAcolher.Infrastructure.Auditing;
using ClinicaAcolher.Infrastructure.Persistence.Repositories;
using ClinicaAcolher.Infrastructure.Security;

namespace ClinicaAcolher.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IClinicalNoteRepository, ClinicalNoteRepository>();
        services.AddScoped<AuditTrail>();
        services.AddScoped<IAuditTrail>(provider => provider.GetRequiredService<AuditTrail>());
        services.AddScoped<AuditChainVerifier>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProfessionalRepository, ProfessionalRepository>();
        services.AddScoped<IRelationshipRepository, RelationshipRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<ISeriesRepository, SeriesRepository>();
        services.AddScoped<IAttendancePolicyRepository, AttendancePolicyRepository>();
        services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();
        services.AddSingleton<IFieldEncryptor, AesGcmFieldEncryptor>();
        return services;
    }
}
