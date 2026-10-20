using ClinicBooking.Application.Doctors;
using ClinicBooking.Domain.Doctors;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Infrastructure.Persistence.Repositories;

internal sealed class DoctorRepository(AppDbContext context) : IDoctorRepository
{
    public async Task<IReadOnlyList<Doctor>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.Doctors
            .AsNoTracking()
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName)
            .ToListAsync(cancellationToken);

    public Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Doctors.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<bool> ExistsByLicenseNumberAsync(string licenseNumber, Guid? excludedId, CancellationToken cancellationToken) =>
        context.Doctors.AnyAsync(
            d => d.LicenseNumber == licenseNumber && (excludedId == null || d.Id != excludedId.Value),
            cancellationToken);

    public Task<bool> ExistsBySpecialtyAsync(Guid specialtyId, CancellationToken cancellationToken) =>
        context.Doctors.AnyAsync(d => d.SpecialtyId == specialtyId, cancellationToken);

    public Task<bool> ExistsByHealthInsuranceAsync(Guid healthInsuranceId, CancellationToken cancellationToken) =>
        context.Doctors.AnyAsync(
            d => d.AcceptedHealthInsurances.Any(a => a.HealthInsuranceId == healthInsuranceId),
            cancellationToken);

    public void Add(Doctor doctor) => context.Doctors.Add(doctor);

    public void Remove(Doctor doctor) => context.Doctors.Remove(doctor);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
