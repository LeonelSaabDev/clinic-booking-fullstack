using ClinicBooking.Application.Patients;
using ClinicBooking.Domain.Patients;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Infrastructure.Persistence.Repositories;

internal sealed class PatientRepository(AppDbContext context) : IPatientRepository
{
    public async Task<IReadOnlyList<Patient>> SearchAsync(string? searchTerm, CancellationToken cancellationToken)
    {
        var query = context.Patients.AsNoTracking();

        if (searchTerm is not null)
        {
            query = query.Where(p =>
                p.FirstName.Contains(searchTerm) ||
                p.LastName.Contains(searchTerm) ||
                p.DocumentNumber.Contains(searchTerm));
        }

        return await query
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync(cancellationToken);
    }

    public Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Patients.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> ExistsByDocumentNumberAsync(string documentNumber, Guid? excludedId, CancellationToken cancellationToken) =>
        context.Patients.AnyAsync(
            p => p.DocumentNumber == documentNumber && (excludedId == null || p.Id != excludedId.Value),
            cancellationToken);

    public Task<bool> ExistsByHealthInsuranceAsync(Guid healthInsuranceId, CancellationToken cancellationToken) =>
        context.Patients.AnyAsync(p => p.HealthInsuranceId == healthInsuranceId, cancellationToken);

    public void Add(Patient patient) => context.Patients.Add(patient);

    public void Remove(Patient patient) => context.Patients.Remove(patient);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
