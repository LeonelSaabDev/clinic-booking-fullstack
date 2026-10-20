using ClinicBooking.Application.Availability;
using ClinicBooking.Application.Common;
using ClinicBooking.Application.Tests.Appointments;
using ClinicBooking.Application.Tests.Doctors;
using ClinicBooking.Domain.Appointments;
using ClinicBooking.Domain.Doctors;
using ClinicBooking.Domain.Patients;
using Microsoft.Extensions.Time.Testing;

namespace ClinicBooking.Application.Tests.Availability;

public class AvailabilityServiceTests
{
    private static readonly TimeSpan ArgentinaOffset = TimeSpan.FromHours(-3);
    private static readonly DateOnly Date = new(2026, 10, 21);

    private readonly FakeDoctorRepository _doctors = new();
    private readonly FakeAppointmentRepository _appointments = new();
    private readonly FakeTimeProvider _clock = new();
    private readonly AvailabilityService _service;
    private readonly Doctor _doctor = Doctor.Create("Ana", "García", "MN 12345", Guid.CreateVersion7());

    public AvailabilityServiceTests()
    {
        _doctor.SetSchedule(30, [ScheduleBlock.Create(Date.DayOfWeek, new TimeOnly(9, 0), new TimeOnly(11, 0))]);
        _doctors.Saved.Add(_doctor);

        var argentina = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
        _service = new AvailabilityService(_doctors, _appointments, _clock, new ClinicTimeZone(argentina));
    }

    private void SetClinicNow(DateOnly date, int hour, int minute) =>
        _clock.SetUtcNow(new DateTimeOffset(date.ToDateTime(new TimeOnly(hour, minute)), ArgentinaOffset));

    [Fact]
    public async Task GetAvailableSlotsAsync_ForFutureDate_ReturnsEverySlotInClinicTime()
    {
        SetClinicNow(Date.AddDays(-1), 12, 0);

        var slots = await _service.GetAvailableSlotsAsync(_doctor.Id, Date, CancellationToken.None);

        Assert.NotNull(slots);
        Assert.Equal(4, slots.Count);
        Assert.Equal(new DateTimeOffset(2026, 10, 21, 9, 0, 0, ArgentinaOffset), slots[0].StartsAt);
        Assert.Equal(new DateTimeOffset(2026, 10, 21, 9, 30, 0, ArgentinaOffset), slots[0].EndsAt);
        Assert.Equal(ArgentinaOffset, slots[0].StartsAt.Offset);
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_ForToday_ExcludesSlotsThatAlreadyStarted()
    {
        SetClinicNow(Date, 9, 10);

        var slots = await _service.GetAvailableSlotsAsync(_doctor.Id, Date, CancellationToken.None);

        Assert.NotNull(slots);
        Assert.Equal(
            [new TimeOnly(9, 30), new TimeOnly(10, 0), new TimeOnly(10, 30)],
            slots.Select(s => TimeOnly.FromDateTime(s.StartsAt.DateTime)));
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_ForPastDate_ReturnsEmpty()
    {
        SetClinicNow(Date.AddDays(1), 8, 0);

        var slots = await _service.GetAvailableSlotsAsync(_doctor.Id, Date, CancellationToken.None);

        Assert.NotNull(slots);
        Assert.Empty(slots);
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_ForDayWithoutSchedule_ReturnsEmpty()
    {
        SetClinicNow(Date.AddDays(-1), 12, 0);

        var slots = await _service.GetAvailableSlotsAsync(_doctor.Id, Date.AddDays(1), CancellationToken.None);

        Assert.NotNull(slots);
        Assert.Empty(slots);
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_ForUnknownDoctor_ReturnsNull()
    {
        var slots = await _service.GetAvailableSlotsAsync(Guid.NewGuid(), Date, CancellationToken.None);

        Assert.Null(slots);
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_ExcludesBookedSlots()
    {
        SetClinicNow(Date.AddDays(-1), 12, 0);
        var patient = Patient.Create("Juan", "Pérez", "30123456", "juan@mail.com", "1155551234");
        var nine = new DateTimeOffset(2026, 10, 21, 9, 0, 0, ArgentinaOffset);
        _appointments.Saved.Add(Appointment.Book(_doctor, patient, nine, useHealthInsurance: false));

        var slots = await _service.GetAvailableSlotsAsync(_doctor.Id, Date, CancellationToken.None);

        Assert.NotNull(slots);
        Assert.Equal(3, slots.Count);
        Assert.DoesNotContain(slots, s => s.StartsAt == nine);
    }
}
