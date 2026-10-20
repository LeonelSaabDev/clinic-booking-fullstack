using ClinicBooking.Application.Doctors;
using ClinicBooking.Domain.Doctors;

namespace ClinicBooking.Application.Tests.Doctors;

internal sealed class FakeDoctorRepository : IDoctorRepository
{
    private readonly List<Doctor> _pending = [];

    public List<Doctor> Saved { get; } = [];

    public Task<IReadOnlyList<Doctor>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Doctor>>(Saved.ToList());

    public Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.FirstOrDefault(d => d.Id == id));

    public Task<bool> ExistsByLicenseNumberAsync(string licenseNumber, Guid? excludedId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Any(d =>
            d.Id != excludedId &&
            string.Equals(d.LicenseNumber, licenseNumber, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> ExistsBySpecialtyAsync(Guid specialtyId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Any(d => d.SpecialtyId == specialtyId));

    public Task<bool> ExistsByHealthInsuranceAsync(Guid healthInsuranceId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Any(d => d.Accepts(healthInsuranceId)));

    public void Add(Doctor doctor) => _pending.Add(doctor);

    public void Remove(Doctor doctor) => Saved.Remove(doctor);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        return Task.CompletedTask;
    }
}
