using ClinicBooking.Application.Common;
using ClinicBooking.Application.Doctors;

namespace ClinicBooking.Application.Availability;

public sealed class AvailabilityService(
    IDoctorRepository doctors,
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

        return doctor.GetSlotStarts(date.DayOfWeek)
            .Select(start => ToClinicInstant(date, start))
            .Where(startsAt => startsAt > now)
            .Select(startsAt => new AvailableSlotResponse(startsAt, startsAt + duration))
            .ToList();
    }

    private DateTimeOffset ToClinicInstant(DateOnly date, TimeOnly time)
    {
        var localDateTime = date.ToDateTime(time);
        var offset = clinicTimeZone.TimeZone.GetUtcOffset(localDateTime);
        return new DateTimeOffset(localDateTime, offset);
    }
}
