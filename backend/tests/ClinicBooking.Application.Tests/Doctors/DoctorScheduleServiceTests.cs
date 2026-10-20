using ClinicBooking.Application.Doctors;
using ClinicBooking.Application.Tests.HealthInsurances;
using ClinicBooking.Application.Tests.Specialties;
using ClinicBooking.Domain.Common;
using ClinicBooking.Domain.Doctors;

namespace ClinicBooking.Application.Tests.Doctors;

public class DoctorScheduleServiceTests
{
    private readonly FakeDoctorRepository _doctors = new();
    private readonly DoctorService _service;
    private readonly Doctor _doctor = Doctor.Create("Ana", "García", "MN 12345", Guid.CreateVersion7());

    public DoctorScheduleServiceTests()
    {
        _doctors.Saved.Add(_doctor);
        _service = new DoctorService(_doctors, new FakeSpecialtyRepository(), new FakeHealthInsuranceRepository());
    }

    [Fact]
    public async Task UpdateScheduleAsync_WithValidBlocks_SavesAndReturnsSchedule()
    {
        var request = new UpdateDoctorScheduleRequest(20,
        [
            new ScheduleBlockDto(DayOfWeek.Monday, new TimeOnly(15, 0), new TimeOnly(19, 0)),
            new ScheduleBlockDto(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(13, 0)),
        ]);

        var response = await _service.UpdateScheduleAsync(_doctor.Id, request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(20, response.AppointmentDurationMinutes);
        Assert.Equal(new TimeOnly(9, 0), response.Blocks[0].Start);
        Assert.Equal(2, _doctor.WeeklySchedule.Count);
    }

    [Fact]
    public async Task UpdateScheduleAsync_WithOverlappingBlocks_ThrowsDomainException()
    {
        var request = new UpdateDoctorScheduleRequest(30,
        [
            new ScheduleBlockDto(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(13, 0)),
            new ScheduleBlockDto(DayOfWeek.Monday, new TimeOnly(12, 0), new TimeOnly(14, 0)),
        ]);

        await Assert.ThrowsAsync<DomainException>(
            () => _service.UpdateScheduleAsync(_doctor.Id, request, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateScheduleAsync_WhenDoctorDoesNotExist_ReturnsNull()
    {
        var request = new UpdateDoctorScheduleRequest(30, []);

        var response = await _service.UpdateScheduleAsync(Guid.NewGuid(), request, CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public async Task GetScheduleAsync_ReturnsBlocksOrderedByDayAndTime()
    {
        _doctor.SetSchedule(30,
        [
            ScheduleBlock.Create(DayOfWeek.Wednesday, new TimeOnly(9, 0), new TimeOnly(12, 0)),
            ScheduleBlock.Create(DayOfWeek.Monday, new TimeOnly(15, 0), new TimeOnly(18, 0)),
            ScheduleBlock.Create(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(12, 0)),
        ]);

        var response = await _service.GetScheduleAsync(_doctor.Id, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(
            [(DayOfWeek.Monday, new TimeOnly(8, 0)), (DayOfWeek.Monday, new TimeOnly(15, 0)), (DayOfWeek.Wednesday, new TimeOnly(9, 0))],
            response.Blocks.Select(b => (b.Day, b.Start)));
    }

    [Fact]
    public async Task GetScheduleAsync_WhenDoctorDoesNotExist_ReturnsNull()
    {
        var response = await _service.GetScheduleAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(response);
    }
}
