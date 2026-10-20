using ClinicBooking.Domain.Common;
using ClinicBooking.Domain.Doctors;

namespace ClinicBooking.Domain.Tests.Doctors;

public class DoctorScheduleTests
{
    private static Doctor NewDoctor() => Doctor.Create("Ana", "García", "MN 12345", Guid.CreateVersion7());

    private static ScheduleBlock Block(DayOfWeek day, int startHour, int startMinute, int endHour, int endMinute) =>
        ScheduleBlock.Create(day, new TimeOnly(startHour, startMinute), new TimeOnly(endHour, endMinute));

    [Fact]
    public void Create_StartsWithEmptyScheduleAndDefaultDuration()
    {
        var doctor = NewDoctor();

        Assert.Empty(doctor.WeeklySchedule);
        Assert.Equal(Doctor.DefaultAppointmentDurationMinutes, doctor.AppointmentDurationMinutes);
    }

    [Fact]
    public void ScheduleBlock_EndingBeforeOrAtItsStart_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Block(DayOfWeek.Monday, 13, 0, 9, 0));
        Assert.Throws<DomainException>(() => Block(DayOfWeek.Monday, 9, 0, 9, 0));
    }

    [Fact]
    public void SetSchedule_WithMorningAndAfternoonBlocks_ReplacesSchedule()
    {
        var doctor = NewDoctor();

        doctor.SetSchedule(30, [Block(DayOfWeek.Monday, 9, 0, 13, 0), Block(DayOfWeek.Monday, 15, 0, 19, 0)]);

        Assert.Equal(30, doctor.AppointmentDurationMinutes);
        Assert.Equal(2, doctor.WeeklySchedule.Count);
    }

    [Fact]
    public void SetSchedule_WithBlocksThatTouch_IsAllowed()
    {
        var doctor = NewDoctor();

        doctor.SetSchedule(30, [Block(DayOfWeek.Monday, 9, 0, 13, 0), Block(DayOfWeek.Monday, 13, 0, 15, 0)]);

        Assert.Equal(2, doctor.WeeklySchedule.Count);
    }

    [Fact]
    public void SetSchedule_WithOverlappingBlocksOnSameDay_ThrowsDomainException()
    {
        var doctor = NewDoctor();

        Assert.Throws<DomainException>(() =>
            doctor.SetSchedule(30, [Block(DayOfWeek.Monday, 9, 0, 13, 0), Block(DayOfWeek.Monday, 12, 0, 16, 0)]));
    }

    [Fact]
    public void SetSchedule_WithSameHoursOnDifferentDays_IsAllowed()
    {
        var doctor = NewDoctor();

        doctor.SetSchedule(30, [Block(DayOfWeek.Monday, 9, 0, 13, 0), Block(DayOfWeek.Tuesday, 9, 0, 13, 0)]);

        Assert.Equal(2, doctor.WeeklySchedule.Count);
    }

    [Fact]
    public void SetSchedule_WithBlockShorterThanAppointmentDuration_ThrowsDomainException()
    {
        var doctor = NewDoctor();

        Assert.Throws<DomainException>(() => doctor.SetSchedule(45, [Block(DayOfWeek.Monday, 9, 0, 9, 30)]));
    }

    [Theory]
    [InlineData(5)]
    [InlineData(125)]
    [InlineData(32)]
    public void SetSchedule_WithInvalidAppointmentDuration_ThrowsDomainException(int duration)
    {
        var doctor = NewDoctor();

        Assert.Throws<DomainException>(() => doctor.SetSchedule(duration, []));
    }

    [Fact]
    public void SetSchedule_WhenInvalid_KeepsPreviousSchedule()
    {
        var doctor = NewDoctor();
        doctor.SetSchedule(30, [Block(DayOfWeek.Monday, 9, 0, 13, 0)]);

        Assert.Throws<DomainException>(() =>
            doctor.SetSchedule(20, [Block(DayOfWeek.Friday, 9, 0, 13, 0), Block(DayOfWeek.Friday, 10, 0, 11, 0)]));

        Assert.Equal(30, doctor.AppointmentDurationMinutes);
        Assert.Equal(DayOfWeek.Monday, Assert.Single(doctor.WeeklySchedule).Day);
    }

    [Fact]
    public void GetSlotStarts_SplitsEachBlockByAppointmentDuration()
    {
        var doctor = NewDoctor();
        doctor.SetSchedule(30, [Block(DayOfWeek.Monday, 15, 0, 16, 0), Block(DayOfWeek.Monday, 9, 0, 10, 0)]);

        var slots = doctor.GetSlotStarts(DayOfWeek.Monday);

        Assert.Equal(
            [new TimeOnly(9, 0), new TimeOnly(9, 30), new TimeOnly(15, 0), new TimeOnly(15, 30)],
            slots);
    }

    [Fact]
    public void GetSlotStarts_IgnoresLeftoverTimeThatDoesNotFitAnAppointment()
    {
        var doctor = NewDoctor();
        doctor.SetSchedule(40, [Block(DayOfWeek.Monday, 9, 0, 10, 30)]);

        var slots = doctor.GetSlotStarts(DayOfWeek.Monday);

        Assert.Equal([new TimeOnly(9, 0), new TimeOnly(9, 40)], slots);
    }

    [Fact]
    public void GetSlotStarts_WithBlockEndingAtEndOfDay_DoesNotWrapPastMidnight()
    {
        var doctor = NewDoctor();
        doctor.SetSchedule(30, [Block(DayOfWeek.Monday, 23, 0, 23, 59)]);

        var slots = doctor.GetSlotStarts(DayOfWeek.Monday);

        Assert.Equal([new TimeOnly(23, 0)], slots);
    }

    [Fact]
    public void GetSlotStarts_ForDayWithoutBlocks_ReturnsEmpty()
    {
        var doctor = NewDoctor();
        doctor.SetSchedule(30, [Block(DayOfWeek.Monday, 9, 0, 13, 0)]);

        Assert.Empty(doctor.GetSlotStarts(DayOfWeek.Sunday));
    }
}
