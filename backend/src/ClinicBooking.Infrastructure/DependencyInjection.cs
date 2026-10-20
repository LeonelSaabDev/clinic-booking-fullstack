using ClinicBooking.Application.Common;
using ClinicBooking.Application.Doctors;
using ClinicBooking.Application.HealthInsurances;
using ClinicBooking.Application.Patients;
using ClinicBooking.Application.Specialties;
using ClinicBooking.Infrastructure.Persistence;
using ClinicBooking.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicBooking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ClinicBookingDb")
            ?? throw new InvalidOperationException("Connection string 'ClinicBookingDb' was not found.");

        var clinicTimeZoneId = configuration["Clinic:TimeZone"]
            ?? throw new InvalidOperationException("Setting 'Clinic:TimeZone' was not found.");

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(new ClinicTimeZone(TimeZoneInfo.FindSystemTimeZoneById(clinicTimeZoneId)));

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
        services.AddScoped<IHealthInsuranceRepository, HealthInsuranceRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();

        return services;
    }
}
