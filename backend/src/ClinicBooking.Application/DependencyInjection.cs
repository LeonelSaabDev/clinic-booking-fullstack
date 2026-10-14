using ClinicBooking.Application.Specialties;
using Microsoft.Extensions.DependencyInjection; 

namespace ClinicBooking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<SpecialtyService>();
        return services;
    }
}
