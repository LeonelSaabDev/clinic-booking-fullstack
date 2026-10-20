using ClinicBooking.Application.Appointments;
using ClinicBooking.Application.Common;
using ClinicBooking.Application.Doctors;

namespace ClinicBooking.Application.Availability;

public sealed class AvailabilityService(
    IDoctorRepository doctors,
    IAppointmentRepository appointments,
    TimeProvider timeProvider,
    ClinicTimeZone clinicTimeZone)
{
    public async Task<IReadOnlyList<AvailableSlotResponse>?> GetAvailableSlotsAsync(
        Guid doctorId, DateOnly date, CancellationToken cancellationToken)
    {
        var doctor = await doctors.GetByIdAsync(doctorId, cancellationToken);
        if (doctor is null)
            return null;

        var now = timeProvider.GetUtcNow();
        var duration = TimeSpan.FromMinutes(doctor.AppointmentDurationMinutes);

        var upcomingStarts = doctor.GetSlotStarts(date.DayOfWeek)
            .Select(start => clinicTimeZone.ToInstant(date, start))
            .Where(startsAt => startsAt > now)
            .ToList();

        if (upcomingStarts.Count == 0)
            return [];

        // DateTimeOffset equality compares instants, so UTC and clinic-time values match.
        var booked = (await appointments.GetScheduledStartsAsync(
                doctorId, upcomingStarts[0], upcomingStarts[^1] + duration, cancellationToken))
            .ToHashSet();

        return upcomingStarts
            .Where(startsAt => !booked.Contains(startsAt))
            .Select(startsAt => new AvailableSlotResponse(startsAt, startsAt + duration))
            .ToList();
    }
}
