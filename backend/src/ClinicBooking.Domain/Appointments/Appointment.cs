using ClinicBooking.Domain.Common;
using ClinicBooking.Domain.Doctors;
using ClinicBooking.Domain.Patients;

namespace ClinicBooking.Domain.Appointments;

public sealed class Appointment
{
    public Guid Id { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid PatientId { get; private set; }

    /// <summary>Always stored in UTC; converted to clinic time only for display.</summary>
    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset EndsAt { get; private set; }
    public AppointmentStatus Status { get; private set; }

    /// <summary>Coverage snapshot: what was used at booking time, not the patient's current insurance.</summary>
    public Guid? HealthInsuranceId { get; private set; }

    public string? MemberNumber { get; private set; }

    private Appointment() { }

    /// <summary>
    /// Books a slot. Checking that the slot is free and inside the schedule needs other appointments,
    /// so that belongs to the application layer; here live the rules this decision can answer alone.
    /// </summary>
    public static Appointment Book(Doctor doctor, Patient patient, DateTimeOffset startsAt, bool useHealthInsurance)
    {
        var appointment = new Appointment
        {
            Id = Guid.CreateVersion7(),
            DoctorId = doctor.Id,
            PatientId = patient.Id,
            StartsAt = startsAt.ToUniversalTime(),
            EndsAt = startsAt.ToUniversalTime().AddMinutes(doctor.AppointmentDurationMinutes),
            Status = AppointmentStatus.Scheduled,
        };

        if (useHealthInsurance)
        {
            if (patient.HealthInsuranceId is not { } healthInsuranceId)
                throw new DomainException("The patient has no health insurance. Book the appointment as private.");

            if (!doctor.Accepts(healthInsuranceId))
                throw new DomainException("The doctor does not accept the patient's health insurance.");

            appointment.HealthInsuranceId = healthInsuranceId;
            appointment.MemberNumber = patient.MemberNumber;
        }

        return appointment;
    }

    public bool Overlaps(DateTimeOffset startsAt, DateTimeOffset endsAt) =>
        StartsAt < endsAt && startsAt < EndsAt;
}
