using ClinicBooking.Domain.HealthInsurances;

namespace ClinicBooking.Application.HealthInsurances;

public interface IHealthInsuranceRepository
{
    Task<IReadOnlyList<HealthInsurance>> GetAllAsync(CancellationToken cancellationToken);
    Task<HealthInsurance?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, Guid? excludedId, CancellationToken cancellationToken);
    void Add(HealthInsurance healthInsurance);
    void Remove(HealthInsurance healthInsurance);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
