namespace ClinicBooking.Application.Common;

/// <summary>The time zone the clinic works in. Schedules are expressed in this local time.</summary>
public sealed record ClinicTimeZone(TimeZoneInfo TimeZone)
{
    /// <summary>Turns a clinic wall-clock time on a given day into an instant carrying the clinic's offset.</summary>
    public DateTimeOffset ToInstant(DateOnly date, TimeOnly time)
    {
        var localDateTime = date.ToDateTime(time);
        return new DateTimeOffset(localDateTime, TimeZone.GetUtcOffset(localDateTime));
    }

    public DateTimeOffset ToClinicTime(DateTimeOffset instant) => TimeZoneInfo.ConvertTime(instant, TimeZone);

    public DateOnly ToClinicDate(DateTimeOffset instant) => DateOnly.FromDateTime(ToClinicTime(instant).DateTime);
}
