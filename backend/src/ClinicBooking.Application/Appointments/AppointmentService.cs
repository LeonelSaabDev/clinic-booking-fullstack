using ClinicBooking.Application.Availability;
using ClinicBooking.Application.Common;
using ClinicBooking.Application.Doctors;
using ClinicBooking.Application.Patients;
using ClinicBooking.Domain.Appointments;

namespace ClinicBooking.Application.Appointments;

public sealed class AppointmentService(
    IAppointmentRepository appointments,
    IDoctorRepository doctors,
    IPatientRepository patients,
    AvailabilityService availability,
    ClinicTimeZone clinicTimeZone)
{
    public async Task<IReadOnlyList<AppointmentResponse>> ListAsync(
        DateOnly date, Guid? doctorId, CancellationToken cancellationToken)
    {
        var from = clinicTimeZone.ToInstant(date, TimeOnly.MinValue);
        var to = clinicTimeZone.ToInstant(date.AddDays(1), TimeOnly.MinValue);

        var found = await appointments.ListAsync(from, to, doctorId, cancellationToken);
        return found.Select(a => AppointmentResponse.FromEntity(a, clinicTimeZone)).ToList();
    }

    public async Task<AppointmentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await appointments.GetByIdAsync(id, cancellationToken);
        return appointment is null ? null : AppointmentResponse.FromEntity(appointment, clinicTimeZone);
    }

    public async Task<AppointmentResponse> BookAsync(BookAppointmentRequest request, CancellationToken cancellationToken)
    {
        var doctor = await doctors.GetByIdAsync(request.DoctorId, cancellationToken)
            ?? throw new InvalidReferenceException("The selected doctor does not exist.");
        var patient = await patients.GetByIdAsync(request.PatientId, cancellationToken)
            ?? throw new InvalidReferenceException("The selected patient does not exist.");

        await EnsureSlotIsAvailableAsync(request.DoctorId, request.StartsAt, cancellationToken);

        var appointment = Appointment.Book(doctor, patient, request.StartsAt, request.UseHealthInsurance);

        if (await appointments.PatientHasScheduledOverlapAsync(
                patient.Id, appointment.StartsAt, appointment.EndsAt, cancellationToken))
            throw new ConflictException("The patient already has another appointment at that time.");

        appointments.Add(appointment);

        // A concurrent booking can still slip past the checks above; the database's unique index
        // is the final guard, and the repository turns its rejection into a ConflictException.
        await appointments.SaveChangesAsync(cancellationToken);

        return AppointmentResponse.FromEntity(appointment, clinicTimeZone);
    }

    private async Task EnsureSlotIsAvailableAsync(Guid doctorId, DateTimeOffset startsAt, CancellationToken cancellationToken)
    {
        var date = clinicTimeZone.ToClinicDate(startsAt);
        var freeSlots = await availability.GetAvailableSlotsAsync(doctorId, date, cancellationToken) ?? [];

        if (freeSlots.Any(slot => slot.StartsAt == startsAt))
            return;

        if (await appointments.IsDoctorBookedAtAsync(doctorId, startsAt, cancellationToken))
            throw new ConflictException("That slot has already been booked.");

        throw new RequestValidationException("The selected time is not an available slot for this doctor.");
    }
}
