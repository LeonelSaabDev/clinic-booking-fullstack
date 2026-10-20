using ClinicBooking.Domain.Patients;

namespace ClinicBooking.Application.Patients;

public sealed record CreatePatientRequest(
    string FirstName,
    string LastName,
    string DocumentNumber,
    string Email,
    string Phone,
    Guid? HealthInsuranceId,
    string? MemberNumber);

public sealed record UpdatePatientRequest(
    string FirstName,
    string LastName,
    string DocumentNumber,
    string Email,
    string Phone,
    Guid? HealthInsuranceId,
    string? MemberNumber);

public sealed record PatientResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string DocumentNumber,
    string Email,
    string Phone,
    Guid? HealthInsuranceId,
    string? MemberNumber)
{
    public static PatientResponse FromEntity(Patient patient) =>
        new(
            patient.Id,
            patient.FirstName,
            patient.LastName,
            patient.DocumentNumber,
            patient.Email,
            patient.Phone,
            patient.HealthInsuranceId,
            patient.MemberNumber);
}
