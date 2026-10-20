namespace ClinicBooking.Application.Common;

/// <summary>The time zone the clinic works in. Schedules are expressed in this local time.</summary>
public sealed record ClinicTimeZone(TimeZoneInfo TimeZone);
