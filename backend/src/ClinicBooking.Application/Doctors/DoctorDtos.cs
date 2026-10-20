using ClinicBooking.Domain.Doctors;

namespace ClinicBooking.Application.Doctors;

public sealed record CreateDoctorRequest(
    string FirstName,
    string LastName,
    string LicenseNumber,
    Guid SpecialtyId,
    IReadOnlyList<Guid>? HealthInsuranceIds);

public sealed record UpdateDoctorRequest(
    string FirstName,
    string LastName,
    string LicenseNumber,
    Guid SpecialtyId,
    IReadOnlyList<Guid>? HealthInsuranceIds);

public sealed record DoctorResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string LicenseNumber,
    Guid SpecialtyId,
    IReadOnlyList<Guid> HealthInsuranceIds)
{
    public static DoctorResponse FromEntity(Doctor doctor) =>
        new(
            doctor.Id,
            doctor.FirstName,
            doctor.LastName,
            doctor.LicenseNumber,
            doctor.SpecialtyId,
            doctor.AcceptedHealthInsurances.Select(a => a.HealthInsuranceId).ToList());
}
