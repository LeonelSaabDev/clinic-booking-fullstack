namespace ClinicBooking.Application.Availability;

/// <summary>A free appointment slot. Both instants carry the clinic's UTC offset, e.g. 2026-10-21T09:00:00-03:00.</summary>
public sealed record AvailableSlotResponse(DateTimeOffset StartsAt, DateTimeOffset EndsAt);
