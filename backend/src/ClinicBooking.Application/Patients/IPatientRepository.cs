using ClinicBooking.Domain.Patients;

namespace ClinicBooking.Application.Patients;

public interface IPatientRepository
{
    /// <param name="searchTerm">Matches first name, last name or document number; null returns everyone.</param>
    Task<IReadOnlyList<Patient>> SearchAsync(string? searchTerm, CancellationToken cancellationToken);
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByDocumentNumberAsync(string documentNumber, Guid? excludedId, CancellationToken cancellationToken);
    Task<bool> ExistsByHealthInsuranceAsync(Guid healthInsuranceId, CancellationToken cancellationToken);
    void Add(Patient patient);
    void Remove(Patient patient);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
