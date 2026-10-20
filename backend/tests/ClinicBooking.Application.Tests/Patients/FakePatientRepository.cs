using ClinicBooking.Application.Patients;
using ClinicBooking.Domain.Patients;

namespace ClinicBooking.Application.Tests.Patients;

internal sealed class FakePatientRepository : IPatientRepository
{
    private readonly List<Patient> _pending = [];

    public List<Patient> Saved { get; } = [];

    public Task<IReadOnlyList<Patient>> SearchAsync(string? searchTerm, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Patient>>(Saved
            .Where(p => searchTerm is null ||
                        p.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        p.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        p.DocumentNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList());

    public Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.FirstOrDefault(p => p.Id == id));

    public Task<bool> ExistsByDocumentNumberAsync(string documentNumber, Guid? excludedId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Any(p => p.Id != excludedId && p.DocumentNumber == documentNumber));

    public Task<bool> ExistsByHealthInsuranceAsync(Guid healthInsuranceId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Any(p => p.HealthInsuranceId == healthInsuranceId));

    public void Add(Patient patient) => _pending.Add(patient);

    public void Remove(Patient patient) => Saved.Remove(patient);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        return Task.CompletedTask;
    }
}
