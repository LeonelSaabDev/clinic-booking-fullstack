using ClinicBooking.Domain.Common;
using ClinicBooking.Domain.Doctors;
using ClinicBooking.Domain.Patients;

namespace ClinicBooking.Domain.Appointments;

public sealed class Appointment
{
    public static readonly TimeSpan MinimumCancellationNotice = TimeSpan.FromHours(24);

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

    /// <summary>Frees the slot. Thanks to the filtered unique index, it can be booked again right away.</summary>
    public void Cancel(DateTimeOffset now)
    {
        EnsureScheduled("cancelled");

        if (now > StartsAt - MinimumCancellationNotice)
            throw new DomainException(
                $"Appointments can only be cancelled at least {MinimumCancellationNotice.TotalHours:0} hours in advance.");

        Status = AppointmentStatus.Cancelled;
    }

    public void Complete(DateTimeOffset now)
    {
        EnsureScheduled("completed");
        EnsureStarted(now, "completed");
        Status = AppointmentStatus.Completed;
    }

    public void MarkNoShow(DateTimeOffset now)
    {
        EnsureScheduled("marked as no-show");
        EnsureStarted(now, "marked as no-show");
        Status = AppointmentStatus.NoShow;
    }

    private void EnsureScheduled(string action)
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new DomainException($"Only scheduled appointments can be {action}. This one is {Status}.");
    }

    private void EnsureStarted(DateTimeOffset now, string action)
    {
        if (now < StartsAt)
            throw new DomainException($"An appointment can only be {action} once it has started.");
    }
}
