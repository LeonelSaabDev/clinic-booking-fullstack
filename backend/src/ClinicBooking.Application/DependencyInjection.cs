using ClinicBooking.Application.Appointments;
using ClinicBooking.Application.Auth;
using ClinicBooking.Application.Availability;
using ClinicBooking.Application.Doctors;
using ClinicBooking.Application.HealthInsurances;
using ClinicBooking.Application.Patients;
using ClinicBooking.Application.Users;
using ClinicBooking.Application.Specialties;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicBooking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<SpecialtyService>();
        services.AddScoped<HealthInsuranceService>();
        services.AddScoped<DoctorService>();
        services.AddScoped<AvailabilityService>();
        services.AddScoped<PatientService>();
        services.AddScoped<AppointmentService>();
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>();

        return services;
    }
}
