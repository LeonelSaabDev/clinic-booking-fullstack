using ClinicBooking.Domain.Appointments;
using ClinicBooking.Domain.Doctors;
using ClinicBooking.Domain.HealthInsurances;
using ClinicBooking.Domain.Patients;
using ClinicBooking.Domain.Specialties;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Specialty> Specialties => Set<Specialty>();

    public DbSet<HealthInsurance> HealthInsurances => Set<HealthInsurance>();

    public DbSet<Doctor> Doctors => Set<Doctor>();

    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
