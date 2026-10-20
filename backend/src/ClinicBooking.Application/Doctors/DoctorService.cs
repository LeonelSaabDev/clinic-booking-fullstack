using ClinicBooking.Application.Common;
using ClinicBooking.Application.HealthInsurances;
using ClinicBooking.Application.Specialties;
using ClinicBooking.Domain.Doctors;

namespace ClinicBooking.Application.Doctors;

public sealed class DoctorService(
    IDoctorRepository doctors,
    ISpecialtyRepository specialties,
    IHealthInsuranceRepository healthInsurances)
{
    public async Task<IReadOnlyList<DoctorResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var allDoctors = await doctors.GetAllAsync(cancellationToken);
        return allDoctors.Select(DoctorResponse.FromEntity).ToList();
    }

    public async Task<DoctorResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctor = await doctors.GetByIdAsync(id, cancellationToken);
        return doctor is null ? null : DoctorResponse.FromEntity(doctor);
    }

    public async Task<DoctorResponse> CreateAsync(CreateDoctorRequest request, CancellationToken cancellationToken)
    {
        var doctor = Doctor.Create(request.FirstName, request.LastName, request.LicenseNumber, request.SpecialtyId);
        doctor.SetAcceptedHealthInsurances(request.HealthInsuranceIds ?? []);

        await EnsureReferencesExistAsync(doctor, cancellationToken);
        await EnsureLicenseNumberIsUniqueAsync(doctor, excludedId: null, cancellationToken);

        doctors.Add(doctor);
        await doctors.SaveChangesAsync(cancellationToken);

        return DoctorResponse.FromEntity(doctor);
    }

    public async Task<DoctorResponse?> UpdateAsync(Guid id, UpdateDoctorRequest request, CancellationToken cancellationToken)
    {
        var doctor = await doctors.GetByIdAsync(id, cancellationToken);
        if (doctor is null)
            return null;

        doctor.UpdateDetails(request.FirstName, request.LastName, request.LicenseNumber, request.SpecialtyId);
        doctor.SetAcceptedHealthInsurances(request.HealthInsuranceIds ?? []);

        await EnsureReferencesExistAsync(doctor, cancellationToken);
        await EnsureLicenseNumberIsUniqueAsync(doctor, excludedId: doctor.Id, cancellationToken);

        await doctors.SaveChangesAsync(cancellationToken);

        return DoctorResponse.FromEntity(doctor);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctor = await doctors.GetByIdAsync(id, cancellationToken);
        if (doctor is null)
            return false;

        doctors.Remove(doctor);
        await doctors.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task EnsureReferencesExistAsync(Doctor doctor, CancellationToken cancellationToken)
    {
        if (await specialties.GetByIdAsync(doctor.SpecialtyId, cancellationToken) is null)
            throw new InvalidReferenceException($"Specialty '{doctor.SpecialtyId}' does not exist.");

        var healthInsuranceIds = doctor.AcceptedHealthInsurances.Select(a => a.HealthInsuranceId).ToList();

        if (!await healthInsurances.AllExistAsync(healthInsuranceIds, cancellationToken))
            throw new InvalidReferenceException("One or more of the selected health insurances do not exist.");
    }

    private async Task EnsureLicenseNumberIsUniqueAsync(Doctor doctor, Guid? excludedId, CancellationToken cancellationToken)
    {
        if (await doctors.ExistsByLicenseNumberAsync(doctor.LicenseNumber, excludedId, cancellationToken))
            throw new ConflictException($"A doctor with license number '{doctor.LicenseNumber}' already exists.");
    }
}
