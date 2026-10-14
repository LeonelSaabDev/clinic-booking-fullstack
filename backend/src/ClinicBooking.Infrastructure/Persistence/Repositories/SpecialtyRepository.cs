using ClinicBooking.Application.Specialties;
using ClinicBooking.Domain.Specialties;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Infrastructure.Persistence.Repositories;

internal sealed class SpecialtyRepository(AppDbContext context) : ISpecialtyRepository
{
    public async Task<IReadOnlyList<Specialty>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.Specialties
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

    public Task<Specialty?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Specialties.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, Guid? excludedId, CancellationToken cancellationToken) =>
        context.Specialties.AnyAsync(
            s => s.Name == name && (excludedId == null || s.Id != excludedId.Value),
            cancellationToken);

    public void Add(Specialty specialty) => context.Specialties.Add(specialty);

    public void Remove(Specialty specialty) => context.Specialties.Remove(specialty);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
