using ClinicBooking.Application.HealthInsurances;
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

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
        services.AddScoped<IHealthInsuranceRepository, HealthInsuranceRepository>();

        return services;
    }
}
