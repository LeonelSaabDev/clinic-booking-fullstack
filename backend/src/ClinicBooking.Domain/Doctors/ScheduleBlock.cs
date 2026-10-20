using ClinicBooking.Domain.Common;

namespace ClinicBooking.Domain.Doctors;

/// <summary>A weekly time range in clinic local time, for example Mondays from 09:00 to 13:00.</summary>
public sealed class ScheduleBlock
{
    public DayOfWeek Day { get; private set; }
    public TimeOnly Start { get; private set; }
    public TimeOnly End { get; private set; }

    public TimeSpan Length => End - Start;

    private ScheduleBlock() { }

    public static ScheduleBlock Create(DayOfWeek day, TimeOnly start, TimeOnly end)
    {
        if (!Enum.IsDefined(day))
            throw new DomainException("Schedule block day is not valid.");

        if (end <= start)
            throw new DomainException("A schedule block must end after it starts.");

        return new ScheduleBlock { Day = day, Start = start, End = end };
    }

    internal bool Overlaps(ScheduleBlock other) =>
        Day == other.Day && Start < other.End && other.Start < End;
}
