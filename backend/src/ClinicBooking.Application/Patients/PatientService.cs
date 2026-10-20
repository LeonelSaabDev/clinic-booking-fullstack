using ClinicBooking.Application.Common;
using ClinicBooking.Application.HealthInsurances;
using ClinicBooking.Domain.Patients;

namespace ClinicBooking.Application.Patients;

public sealed class PatientService(IPatientRepository patients, IHealthInsuranceRepository healthInsurances)
{
    public async Task<IReadOnlyList<PatientResponse>> SearchAsync(string? searchTerm, CancellationToken cancellationToken)
    {
        var term = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
        var found = await patients.SearchAsync(term, cancellationToken);
        return found.Select(PatientResponse.FromEntity).ToList();
    }

    public async Task<PatientResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var patient = await patients.GetByIdAsync(id, cancellationToken);
        return patient is null ? null : PatientResponse.FromEntity(patient);
    }

    public async Task<PatientResponse> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken)
    {
        var patient = Patient.Create(request.FirstName, request.LastName, request.DocumentNumber, request.Email, request.Phone);
        patient.SetCoverage(request.HealthInsuranceId, request.MemberNumber);

        await EnsureHealthInsuranceExistsAsync(patient, cancellationToken);
        await EnsureDocumentNumberIsUniqueAsync(patient, excludedId: null, cancellationToken);

        patients.Add(patient);
        await patients.SaveChangesAsync(cancellationToken);

        return PatientResponse.FromEntity(patient);
    }

    public async Task<PatientResponse?> UpdateAsync(Guid id, UpdatePatientRequest request, CancellationToken cancellationToken)
    {
        var patient = await patients.GetByIdAsync(id, cancellationToken);
        if (patient is null)
            return null;

        patient.UpdateDetails(request.FirstName, request.LastName, request.DocumentNumber, request.Email, request.Phone);
        patient.SetCoverage(request.HealthInsuranceId, request.MemberNumber);

        await EnsureHealthInsuranceExistsAsync(patient, cancellationToken);
        await EnsureDocumentNumberIsUniqueAsync(patient, excludedId: patient.Id, cancellationToken);

        await patients.SaveChangesAsync(cancellationToken);

        return PatientResponse.FromEntity(patient);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var patient = await patients.GetByIdAsync(id, cancellationToken);
        if (patient is null)
            return false;

        patients.Remove(patient);
        await patients.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task EnsureHealthInsuranceExistsAsync(Patient patient, CancellationToken cancellationToken)
    {
        if (patient.HealthInsuranceId is { } healthInsuranceId &&
            !await healthInsurances.AllExistAsync([healthInsuranceId], cancellationToken))
            throw new InvalidReferenceException("The selected health insurance does not exist.");
    }

    private async Task EnsureDocumentNumberIsUniqueAsync(Patient patient, Guid? excludedId, CancellationToken cancellationToken)
    {
        if (await patients.ExistsByDocumentNumberAsync(patient.DocumentNumber, excludedId, cancellationToken))
            throw new ConflictException($"A patient with document number '{patient.DocumentNumber}' already exists.");
    }
}
