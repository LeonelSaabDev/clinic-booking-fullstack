using ClinicBooking.Application.Common;
using ClinicBooking.Application.Doctors;
using ClinicBooking.Application.Tests.Appointments;
using ClinicBooking.Application.Tests.HealthInsurances;
using ClinicBooking.Application.Tests.Specialties;
using ClinicBooking.Domain.Appointments;
using ClinicBooking.Domain.Doctors;
using ClinicBooking.Domain.HealthInsurances;
using ClinicBooking.Domain.Patients;
using ClinicBooking.Domain.Specialties;

namespace ClinicBooking.Application.Tests.Doctors;

public class DoctorServiceTests
{
    private readonly FakeDoctorRepository _doctors = new();
    private readonly FakeSpecialtyRepository _specialties = new();
    private readonly FakeHealthInsuranceRepository _healthInsurances = new();
    private readonly FakeAppointmentRepository _appointments = new();
    private readonly DoctorService _service;

    private readonly Specialty _cardiology = Specialty.Create("Cardiology");
    private readonly HealthInsurance _osde = HealthInsurance.Create("OSDE");

    public DoctorServiceTests()
    {
        _specialties.Saved.Add(_cardiology);
        _healthInsurances.Saved.Add(_osde);
        _service = new DoctorService(_doctors, _specialties, _healthInsurances, _appointments);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_SavesAndReturnsDoctor()
    {
        var request = new CreateDoctorRequest("Ana", "García", "MN 12345", _cardiology.Id, [_osde.Id]);

        var response = await _service.CreateAsync(request, CancellationToken.None);

        Assert.Equal("Ana", response.FirstName);
        Assert.Equal("García", response.LastName);
        Assert.Equal("MN 12345", response.LicenseNumber);
        Assert.Equal(_cardiology.Id, response.SpecialtyId);
        Assert.Equal([_osde.Id], response.HealthInsuranceIds);
        var saved = Assert.Single(_doctors.Saved);
        Assert.Equal(response.Id, saved.Id);
    }

    [Fact]
    public async Task CreateAsync_WithoutHealthInsurances_SavesPrivateOnlyDoctor()
    {
        var request = new CreateDoctorRequest("Ana", "García", "MN 12345", _cardiology.Id, []);

        var response = await _service.CreateAsync(request, CancellationToken.None);

        Assert.Empty(response.HealthInsuranceIds);
        Assert.Single(_doctors.Saved);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateLicenseNumberAndExtraSpaces_ThrowsConflictException()
    {
        _doctors.Saved.Add(Doctor.Create("Laura", "Pérez", "MN 12345", _cardiology.Id));
        var request = new CreateDoctorRequest("Ana", "García", "  mn 12345  ", _cardiology.Id, []);

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithUnknownSpecialty_ThrowsInvalidReferenceException()
    {
        var request = new CreateDoctorRequest("Ana", "García", "MN 12345", Guid.NewGuid(), []);

        await Assert.ThrowsAsync<InvalidReferenceException>(
            () => _service.CreateAsync(request, CancellationToken.None));
        Assert.Empty(_doctors.Saved);
    }

    [Fact]
    public async Task CreateAsync_WithUnknownHealthInsurance_ThrowsInvalidReferenceException()
    {
        var request = new CreateDoctorRequest("Ana", "García", "MN 12345", _cardiology.Id, [_osde.Id, Guid.NewGuid()]);

        await Assert.ThrowsAsync<InvalidReferenceException>(
            () => _service.CreateAsync(request, CancellationToken.None));
        Assert.Empty(_doctors.Saved);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesAndReturnsDoctor()
    {
        var doctor = Doctor.Create("Ana", "García", "MN 12345", _cardiology.Id);
        _doctors.Saved.Add(doctor);
        var pediatrics = Specialty.Create("Pediatrics");
        _specialties.Saved.Add(pediatrics);
        var request = new UpdateDoctorRequest("Ana", "García López", "MN 12345", pediatrics.Id, [_osde.Id]);

        var response = await _service.UpdateAsync(doctor.Id, request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal("García López", response.LastName);
        Assert.Equal(pediatrics.Id, response.SpecialtyId);
        Assert.True(doctor.Accepts(_osde.Id));
    }

    [Fact]
    public async Task UpdateAsync_WhenDoctorDoesNotExist_ReturnsNull()
    {
        var request = new UpdateDoctorRequest("Ana", "García", "MN 12345", _cardiology.Id, []);

        var response = await _service.UpdateAsync(Guid.NewGuid(), request, CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public async Task UpdateAsync_WithLicenseNumberOfAnotherDoctor_ThrowsConflictException()
    {
        var ana = Doctor.Create("Ana", "García", "MN 12345", _cardiology.Id);
        _doctors.Saved.Add(ana);
        _doctors.Saved.Add(Doctor.Create("Laura", "Pérez", "MN 999", _cardiology.Id));
        var request = new UpdateDoctorRequest("Ana", "García", "MN 999", _cardiology.Id, []);

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.UpdateAsync(ana.Id, request, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_KeepingOwnLicenseNumber_UpdatesDoctor()
    {
        var ana = Doctor.Create("Ana", "García", "MN 12345", _cardiology.Id);
        _doctors.Saved.Add(ana);
        var request = new UpdateDoctorRequest("Ana María", "García", "mn 12345", _cardiology.Id, []);

        var response = await _service.UpdateAsync(ana.Id, request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal("Ana María", response.FirstName);
    }

    [Fact]
    public async Task UpdateAsync_WithUnknownSpecialty_ThrowsInvalidReferenceException()
    {
        var ana = Doctor.Create("Ana", "García", "MN 12345", _cardiology.Id);
        _doctors.Saved.Add(ana);
        var request = new UpdateDoctorRequest("Ana", "García", "MN 12345", Guid.NewGuid(), []);

        await Assert.ThrowsAsync<InvalidReferenceException>(
            () => _service.UpdateAsync(ana.Id, request, CancellationToken.None));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllDoctors()
    {
        _doctors.Saved.Add(Doctor.Create("Ana", "García", "MN 1", _cardiology.Id));
        _doctors.Saved.Add(Doctor.Create("Laura", "Pérez", "MN 2", _cardiology.Id));

        var response = await _service.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, response.Count);
        Assert.Contains(response, d => d.LastName == "García");
        Assert.Contains(response, d => d.LastName == "Pérez");
    }

    [Fact]
    public async Task GetByIdAsync_WhenDoctorExists_ReturnsDoctor()
    {
        var ana = Doctor.Create("Ana", "García", "MN 12345", _cardiology.Id);
        _doctors.Saved.Add(ana);

        var response = await _service.GetByIdAsync(ana.Id, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(ana.Id, response.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenDoctorDoesNotExist_ReturnsNull()
    {
        var response = await _service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public async Task DeleteAsync_WhenDoctorExists_RemovesItAndReturnsTrue()
    {
        var ana = Doctor.Create("Ana", "García", "MN 12345", _cardiology.Id);
        _doctors.Saved.Add(ana);

        var deleted = await _service.DeleteAsync(ana.Id, CancellationToken.None);

        Assert.True(deleted);
        Assert.Empty(_doctors.Saved);
    }

    [Fact]
    public async Task DeleteAsync_WhenDoctorDoesNotExist_ReturnsFalse()
    {
        var deleted = await _service.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(deleted);
    }

    [Fact]
    public async Task DeleteAsync_WhenDoctorHasAppointments_ThrowsConflictException()
    {
        var ana = Doctor.Create("Ana", "García", "MN 12345", _cardiology.Id);
        _doctors.Saved.Add(ana);
        var patient = Patient.Create("Juan", "Pérez", "30123456", "juan@mail.com", "1155551234");
        _appointments.Saved.Add(Appointment.Book(ana, patient, DateTimeOffset.UtcNow.AddDays(1), false));

        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(ana.Id, CancellationToken.None));
        Assert.Single(_doctors.Saved);
    }
}
