using ClinicaAcolher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ClinicaAcolher.Application.Patients;
using ClinicaAcolher.Infrastructure.Persistence.Repositories;

namespace ClinicaAcolher.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IPatientRepository, PatientRepository>();
        return services;
    }
}