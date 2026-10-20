using ClinicBooking.Application.Common;
using ClinicBooking.Application.Patients;
using ClinicBooking.Application.Tests.HealthInsurances;
using ClinicBooking.Domain.HealthInsurances;
using ClinicBooking.Domain.Patients;

namespace ClinicBooking.Application.Tests.Patients;

public class PatientServiceTests
{
    private readonly FakePatientRepository _patients = new();
    private readonly FakeHealthInsuranceRepository _healthInsurances = new();
    private readonly PatientService _service;
    private readonly HealthInsurance _osde = HealthInsurance.Create("OSDE");

    public PatientServiceTests()
    {
        _healthInsurances.Saved.Add(_osde);
        _service = new PatientService(_patients, _healthInsurances);
    }

    private static CreatePatientRequest NewRequest(
        string documentNumber = "30123456", Guid? healthInsuranceId = null, string? memberNumber = null) =>
        new("Juan", "Pérez", documentNumber, "juan@mail.com", "1155551234", healthInsuranceId, memberNumber);

    private static Patient ExistingPatient(string documentNumber) =>
        Patient.Create("Laura", "Gómez", documentNumber, "laura@mail.com", "1144443333");

    [Fact]
    public async Task CreateAsync_WithHealthInsurance_SavesAndReturnsPatient()
    {
        var response = await _service.CreateAsync(NewRequest(healthInsuranceId: _osde.Id, memberNumber: "210-555"),
            CancellationToken.None);

        Assert.Equal("30123456", response.DocumentNumber);
        Assert.Equal(_osde.Id, response.HealthInsuranceId);
        Assert.Equal("210-555", response.MemberNumber);
        Assert.Equal(response.Id, Assert.Single(_patients.Saved).Id);
    }

    [Fact]
    public async Task CreateAsync_WithoutHealthInsurance_SavesPrivatePatient()
    {
        var response = await _service.CreateAsync(NewRequest(), CancellationToken.None);

        Assert.Null(response.HealthInsuranceId);
        Assert.Null(response.MemberNumber);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateDocumentNumberTypedWithDots_ThrowsConflictException()
    {
        _patients.Saved.Add(ExistingPatient("30123456"));

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(NewRequest(documentNumber: "30.123.456"), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithUnknownHealthInsurance_ThrowsInvalidReferenceException()
    {
        await Assert.ThrowsAsync<InvalidReferenceException>(() =>
            _service.CreateAsync(NewRequest(healthInsuranceId: Guid.NewGuid(), memberNumber: "1"), CancellationToken.None));
        Assert.Empty(_patients.Saved);
    }

    [Fact]
    public async Task UpdateAsync_KeepingOwnDocumentNumber_UpdatesPatient()
    {
        var laura = ExistingPatient("30123456");
        _patients.Saved.Add(laura);
        var request = new UpdatePatientRequest("Laura", "Gómez", "30123456", "laura@new.com", "1144443333", null, null);

        var response = await _service.UpdateAsync(laura.Id, request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal("laura@new.com", response.Email);
    }

    [Fact]
    public async Task UpdateAsync_WithDocumentNumberOfAnotherPatient_ThrowsConflictException()
    {
        var laura = ExistingPatient("30123456");
        _patients.Saved.Add(laura);
        _patients.Saved.Add(ExistingPatient("40111222"));
        var request = new UpdatePatientRequest("Laura", "Gómez", "40111222", "laura@mail.com", "1144443333", null, null);

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.UpdateAsync(laura.Id, request, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_WhenPatientDoesNotExist_ReturnsNull()
    {
        var request = new UpdatePatientRequest("Laura", "Gómez", "30123456", "laura@mail.com", "1144443333", null, null);

        Assert.Null(await _service.UpdateAsync(Guid.NewGuid(), request, CancellationToken.None));
    }

    [Fact]
    public async Task SearchAsync_ByDocumentNumberFragment_ReturnsMatchingPatients()
    {
        _patients.Saved.Add(ExistingPatient("30123456"));
        _patients.Saved.Add(ExistingPatient("40111222"));

        var found = await _service.SearchAsync("  3012 ", CancellationToken.None);

        Assert.Equal("30123456", Assert.Single(found).DocumentNumber);
    }

    [Fact]
    public async Task SearchAsync_WithBlankTerm_ReturnsEveryone()
    {
        _patients.Saved.Add(ExistingPatient("30123456"));
        _patients.Saved.Add(ExistingPatient("40111222"));

        var found = await _service.SearchAsync("   ", CancellationToken.None);

        Assert.Equal(2, found.Count);
    }

    [Fact]
    public async Task DeleteAsync_WhenPatientExists_RemovesItAndReturnsTrue()
    {
        var laura = ExistingPatient("30123456");
        _patients.Saved.Add(laura);

        Assert.True(await _service.DeleteAsync(laura.Id, CancellationToken.None));
        Assert.Empty(_patients.Saved);
    }

    [Fact]
    public async Task DeleteAsync_WhenPatientDoesNotExist_ReturnsFalse()
    {
        Assert.False(await _service.DeleteAsync(Guid.NewGuid(), CancellationToken.None));
    }
}
