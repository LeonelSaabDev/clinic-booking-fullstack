using ClinicBooking.Application.Appointments;
using ClinicBooking.Application.Availability;
using ClinicBooking.Application.Common;
using ClinicBooking.Application.Tests.Doctors;
using ClinicBooking.Application.Tests.Patients;
using ClinicBooking.Domain.Appointments;
using ClinicBooking.Domain.Common;
using ClinicBooking.Domain.Doctors;
using ClinicBooking.Domain.Patients;
using Microsoft.Extensions.Time.Testing;

namespace ClinicBooking.Application.Tests.Appointments;

public class AppointmentServiceTests
{
    private static readonly TimeSpan Argentina = TimeSpan.FromHours(-3);
    private static readonly DateOnly Date = new(2026, 10, 21);
    private static readonly Guid Osde = Guid.CreateVersion7();

    private readonly FakeAppointmentRepository _appointments = new();
    private readonly FakeDoctorRepository _doctors = new();
    private readonly FakePatientRepository _patients = new();
    private readonly FakeTimeProvider _clock = new();
    private readonly AppointmentService _service;

    private readonly Doctor _ana = Doctor.Create("Ana", "García", "MN 1", Guid.CreateVersion7());
    private readonly Patient _juan = Patient.Create("Juan", "Pérez", "30123456", "juan@mail.com", "1155551234");
    private readonly Patient _laura = Patient.Create("Laura", "Gómez", "40111222", "laura@mail.com", "1144443333");

    public AppointmentServiceTests()
    {
        _ana.SetSchedule(30, [ScheduleBlock.Create(Date.DayOfWeek, new TimeOnly(9, 0), new TimeOnly(11, 0))]);
        _ana.SetAcceptedHealthInsurances([Osde]);
        _juan.SetCoverage(Osde, "210-555");
        _doctors.Saved.Add(_ana);
        _patients.Saved.AddRange([_juan, _laura]);
        SetClinicNow(Date.AddDays(-2), 8, 0);

        var clinicTimeZone = new ClinicTimeZone(TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires"));
        var availability = new AvailabilityService(_doctors, _appointments, _clock, clinicTimeZone);
        _service = new AppointmentService(_appointments, _doctors, _patients, availability, _clock, clinicTimeZone);
    }

    private void SetClinicNow(DateOnly date, int hour, int minute) =>
        _clock.SetUtcNow(new DateTimeOffset(date.ToDateTime(new TimeOnly(hour, minute)), Argentina));

    private static DateTimeOffset At(int hour, int minute) => new(Date.ToDateTime(new TimeOnly(hour, minute)), Argentina);

    private BookAppointmentRequest Request(Patient patient, DateTimeOffset startsAt, bool useHealthInsurance = false) =>
        new(_ana.Id, patient.Id, startsAt, useHealthInsurance);

    [Fact]
    public async Task BookAsync_FreeSlotAsPrivate_SavesAppointmentInClinicTime()
    {
        var response = await _service.BookAsync(Request(_laura, At(9, 30)), CancellationToken.None);

        Assert.Equal(At(9, 30), response.StartsAt);
        Assert.Equal(Argentina, response.StartsAt.Offset);
        Assert.Equal(At(10, 0), response.EndsAt);
        Assert.Equal(AppointmentStatus.Scheduled, response.Status);
        Assert.Null(response.HealthInsuranceId);
        Assert.Single(_appointments.Saved);
    }

    [Fact]
    public async Task BookAsync_WithHealthInsurance_CopiesCoverage()
    {
        var response = await _service.BookAsync(Request(_juan, At(9, 0), useHealthInsurance: true), CancellationToken.None);

        Assert.Equal(Osde, response.HealthInsuranceId);
        Assert.Equal("210-555", response.MemberNumber);
    }

    [Fact]
    public async Task BookAsync_SlotAlreadyBooked_ThrowsConflictException()
    {
        _appointments.Saved.Add(Appointment.Book(_ana, _laura, At(9, 0), false));

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.BookAsync(Request(_juan, At(9, 0)), CancellationToken.None));
    }

    [Theory]
    [InlineData(12, 0)]
    [InlineData(9, 10)]
    public async Task BookAsync_TimeThatIsNotASlot_ThrowsRequestValidationException(int hour, int minute)
    {
        await Assert.ThrowsAsync<RequestValidationException>(
            () => _service.BookAsync(Request(_juan, At(hour, minute)), CancellationToken.None));
    }

    [Fact]
    public async Task BookAsync_SlotThatAlreadyStarted_ThrowsRequestValidationException()
    {
        SetClinicNow(Date, 9, 5);

        await Assert.ThrowsAsync<RequestValidationException>(
            () => _service.BookAsync(Request(_juan, At(9, 0)), CancellationToken.None));
    }

    [Fact]
    public async Task BookAsync_PatientAlreadyBusyWithAnotherDoctor_ThrowsConflictException()
    {
        var otherDoctor = Doctor.Create("Luis", "Ruiz", "MN 2", Guid.CreateVersion7());
        otherDoctor.SetSchedule(30, [ScheduleBlock.Create(Date.DayOfWeek, new TimeOnly(9, 0), new TimeOnly(11, 0))]);
        _doctors.Saved.Add(otherDoctor);
        _appointments.Saved.Add(Appointment.Book(otherDoctor, _juan, At(9, 0), false));

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.BookAsync(Request(_juan, At(9, 0)), CancellationToken.None));
    }

    [Fact]
    public async Task BookAsync_HealthInsuranceTheDoctorDoesNotAccept_ThrowsDomainException()
    {
        _laura.SetCoverage(Guid.CreateVersion7(), "777");

        await Assert.ThrowsAsync<DomainException>(
            () => _service.BookAsync(Request(_laura, At(9, 0), useHealthInsurance: true), CancellationToken.None));
    }

    [Fact]
    public async Task BookAsync_UnknownDoctor_ThrowsInvalidReferenceException()
    {
        var request = new BookAppointmentRequest(Guid.NewGuid(), _juan.Id, At(9, 0), false);

        await Assert.ThrowsAsync<InvalidReferenceException>(() => _service.BookAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task BookAsync_UnknownPatient_ThrowsInvalidReferenceException()
    {
        var request = new BookAppointmentRequest(_ana.Id, Guid.NewGuid(), At(9, 0), false);

        await Assert.ThrowsAsync<InvalidReferenceException>(() => _service.BookAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task ListAsync_ReturnsTheDayAppointmentsInOrder()
    {
        _appointments.Saved.Add(Appointment.Book(_ana, _laura, At(10, 0), false));
        _appointments.Saved.Add(Appointment.Book(_ana, _juan, At(9, 0), false));
        _appointments.Saved.Add(Appointment.Book(_ana, _juan, At(9, 0).AddDays(7), false));

        var found = await _service.ListAsync(Date, doctorId: null, CancellationToken.None);

        Assert.Equal([At(9, 0), At(10, 0)], found.Select(a => a.StartsAt));
    }

    [Fact]
    public async Task ListAsync_FiltersByDoctor()
    {
        _appointments.Saved.Add(Appointment.Book(_ana, _juan, At(9, 0), false));

        var found = await _service.ListAsync(Date, doctorId: Guid.NewGuid(), CancellationToken.None);

        Assert.Empty(found);
    }

    [Fact]
    public async Task CancelAsync_WithEnoughNotice_FreesTheSlotForAnotherPatient()
    {
        var booked = await _service.BookAsync(Request(_laura, At(9, 0)), CancellationToken.None);

        var cancelled = await _service.CancelAsync(booked.Id, CancellationToken.None);
        var rebooked = await _service.BookAsync(Request(_juan, At(9, 0)), CancellationToken.None);

        Assert.Equal(AppointmentStatus.Cancelled, cancelled?.Status);
        Assert.Equal(At(9, 0), rebooked.StartsAt);
    }

    [Fact]
    public async Task CancelAsync_WithLessThanTheMinimumNotice_ThrowsDomainException()
    {
        var booked = await _service.BookAsync(Request(_laura, At(9, 0)), CancellationToken.None);
        SetClinicNow(Date, 8, 0);

        await Assert.ThrowsAsync<DomainException>(() => _service.CancelAsync(booked.Id, CancellationToken.None));
    }

    [Fact]
    public async Task CompleteAsync_OnceStarted_CompletesTheAppointment()
    {
        var booked = await _service.BookAsync(Request(_laura, At(9, 0)), CancellationToken.None);
        SetClinicNow(Date, 9, 10);

        var completed = await _service.CompleteAsync(booked.Id, CancellationToken.None);

        Assert.Equal(AppointmentStatus.Completed, completed?.Status);
    }

    [Fact]
    public async Task MarkNoShowAsync_OnceStarted_MarksTheAppointment()
    {
        var booked = await _service.BookAsync(Request(_laura, At(9, 0)), CancellationToken.None);
        SetClinicNow(Date, 9, 20);

        var noShow = await _service.MarkNoShowAsync(booked.Id, CancellationToken.None);

        Assert.Equal(AppointmentStatus.NoShow, noShow?.Status);
    }

    [Fact]
    public async Task CancelAsync_WhenAppointmentDoesNotExist_ReturnsNull()
    {
        Assert.Null(await _service.CancelAsync(Guid.NewGuid(), CancellationToken.None));
    }
}
