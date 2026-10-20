using ClinicBooking.Domain.Doctors;

namespace ClinicBooking.Application.Doctors;

public interface IDoctorRepository
{
    Task<IReadOnlyList<Doctor>> GetAllAsync(CancellationToken cancellationToken);
    Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByLicenseNumberAsync(string licenseNumber, Guid? excludedId, CancellationToken cancellationToken);
    Task<bool> ExistsBySpecialtyAsync(Guid specialtyId, CancellationToken cancellationToken);
    Task<bool> ExistsByHealthInsuranceAsync(Guid healthInsuranceId, CancellationToken cancellationToken);
    void Add(Doctor doctor);
    void Remove(Doctor doctor);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
