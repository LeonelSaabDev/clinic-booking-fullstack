using ClinicBooking.Application.Common;
using ClinicBooking.Domain.Appointments;

namespace ClinicBooking.Application.Appointments;

public sealed record BookAppointmentRequest(Guid DoctorId, Guid PatientId, DateTimeOffset StartsAt, bool UseHealthInsurance);

public sealed record AppointmentResponse(
    Guid Id,
    Guid DoctorId,
    Guid PatientId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    AppointmentStatus Status,
    Guid? HealthInsuranceId,
    string? MemberNumber)
{
    /// <summary>Instants are returned in clinic time, e.g. 2026-10-21T09:00:00-03:00.</summary>
    public static AppointmentResponse FromEntity(Appointment appointment, ClinicTimeZone clinicTimeZone) =>
        new(
            appointment.Id,
            appointment.DoctorId,
            appointment.PatientId,
            clinicTimeZone.ToClinicTime(appointment.StartsAt),
            clinicTimeZone.ToClinicTime(appointment.EndsAt),
            appointment.Status,
            appointment.HealthInsuranceId,
            appointment.MemberNumber);
}
