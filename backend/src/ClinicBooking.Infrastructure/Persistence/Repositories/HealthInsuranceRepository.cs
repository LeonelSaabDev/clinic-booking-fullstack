using ClinicBooking.Application.HealthInsurances;
using ClinicBooking.Domain.HealthInsurances;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Infrastructure.Persistence.Repositories;

internal sealed class HealthInsuranceRepository(AppDbContext context) : IHealthInsuranceRepository
{
    public async Task<IReadOnlyList<HealthInsurance>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.HealthInsurances
            .AsNoTracking()
            .OrderBy(h => h.Name)
            .ToListAsync(cancellationToken);

    public Task<HealthInsurance?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.HealthInsurances.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, Guid? excludedId, CancellationToken cancellationToken) =>
        context.HealthInsurances.AnyAsync(
            h => h.Name == name && (excludedId == null || h.Id != excludedId.Value),
            cancellationToken);

    public async Task<bool> AllExistAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
            return true;

        var existingCount = await context.HealthInsurances.CountAsync(h => ids.Contains(h.Id), cancellationToken);
        return existingCount == ids.Count;
    }

    public void Add(HealthInsurance healthInsurance) => context.HealthInsurances.Add(healthInsurance);

    public void Remove(HealthInsurance healthInsurance) => context.HealthInsurances.Remove(healthInsurance);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
