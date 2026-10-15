using ClinicBooking.Application.HealthInsurances;
using ClinicBooking.Domain.HealthInsurances;

namespace ClinicBooking.Application.Tests.HealthInsurances;

internal sealed class FakeHealthInsuranceRepository : IHealthInsuranceRepository
{
    private readonly List<HealthInsurance> _pending = [];

    public List<HealthInsurance> Saved { get; } = [];

    public Task<IReadOnlyList<HealthInsurance>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<HealthInsurance>>(Saved.ToList());

    public Task<HealthInsurance?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.FirstOrDefault(h => h.Id == id));

    public Task<bool> ExistsByNameAsync(string name, Guid? excludedId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Any(h =>
            h.Id != excludedId &&
            string.Equals(h.Name, name, StringComparison.OrdinalIgnoreCase)));

    public void Add(HealthInsurance healthInsurance) => _pending.Add(healthInsurance);

    public void Remove(HealthInsurance healthInsurance) => Saved.Remove(healthInsurance);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        return Task.CompletedTask;
    }
}
