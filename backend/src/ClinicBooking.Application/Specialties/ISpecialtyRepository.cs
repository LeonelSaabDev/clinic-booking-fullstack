using ClinicBooking.Domain.Specialties;

namespace ClinicBooking.Application.Specialties;

public interface ISpecialtyRepository
{
    Task<IReadOnlyList<Specialty>> GetAllAsync(CancellationToken cancellationToken);
    Task<Specialty?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, Guid? excludedId, CancellationToken cancellationToken);
    void Add(Specialty specialty);
    void Remove(Specialty specialty);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
