using ClinicBooking.Application.Specialties;
using ClinicBooking.Domain.Specialties;

namespace ClinicBooking.Application.Tests.Specialties;

internal sealed class FakeSpecialtyRepository : ISpecialtyRepository
{
    private readonly List<Specialty> _pending = [];

    public List<Specialty> Saved { get; } = [];

    public Task<IReadOnlyList<Specialty>> GetAllAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Specialty>>(Saved.ToList());

    public Task<Specialty?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Saved.FirstOrDefault(s => s.Id == id));

    public Task<bool> ExistsByNameAsync(string name, Guid? excludedId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Any(s =>
            s.Id != excludedId &&
            string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)));

    public void Add(Specialty specialty) => _pending.Add(specialty);

    public void Remove(Specialty specialty) => Saved.Remove(specialty);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        return Task.CompletedTask;
    }
}
