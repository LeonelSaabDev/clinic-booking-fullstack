using ClinicBooking.Application.Common;
using ClinicBooking.Application.HealthInsurances;
using ClinicBooking.Domain.HealthInsurances;

namespace ClinicBooking.Application.Tests.HealthInsurances;

public class HealthInsuranceServiceTests
{
    private readonly FakeHealthInsuranceRepository _repository = new();
    private readonly HealthInsuranceService _service;

    public HealthInsuranceServiceTests()
    {
        _service = new HealthInsuranceService(_repository);
    }

    [Fact]
    public async Task CreateAsync_WithUniqueName_SavesAndReturnsHealthInsurance()
    {
        var request = new CreateHealthInsuranceRequest("OSDE");

        var response = await _service.CreateAsync(request, CancellationToken.None);

        Assert.Equal("OSDE", response.Name);
        var saved = Assert.Single(_repository.Saved);
        Assert.Equal(response.Id, saved.Id);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ThrowsConflictException()
    {
        _repository.Saved.Add(HealthInsurance.Create("OSDE"));
        var request = new CreateHealthInsuranceRequest("OSDE");

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateNameAndExtraSpaces_ThrowsConflictException()
    {
        _repository.Saved.Add(HealthInsurance.Create("OSDE"));
        var request = new CreateHealthInsuranceRequest("  OSDE  ");

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task RenameAsync_WithNewUniqueName_UpdatesAndReturnsHealthInsurance()
    {
        var osde = HealthInsurance.Create("OSDE");
        _repository.Saved.Add(osde);
        var request = new RenameHealthInsuranceRequest("Swiss Medical");

        var response = await _service.RenameAsync(osde.Id, request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal("Swiss Medical", response.Name);
        Assert.Equal("Swiss Medical", osde.Name);
    }

    [Fact]
    public async Task RenameAsync_WhenHealthInsuranceDoesNotExist_ReturnsNull()
    {
        var request = new RenameHealthInsuranceRequest("Swiss Medical");

        var response = await _service.RenameAsync(Guid.NewGuid(), request, CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public async Task RenameAsync_WithNameOfAnotherHealthInsurance_ThrowsConflictException()
    {
        var osde = HealthInsurance.Create("OSDE");
        _repository.Saved.Add(osde);
        _repository.Saved.Add(HealthInsurance.Create("Swiss Medical"));
        var request = new RenameHealthInsuranceRequest("Swiss Medical");

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.RenameAsync(osde.Id, request, CancellationToken.None));
    }

    [Fact]
    public async Task RenameAsync_WithNameOfAnotherHealthInsuranceAndExtraSpaces_ThrowsConflictException()
    {
        var osde = HealthInsurance.Create("OSDE");
        _repository.Saved.Add(osde);
        _repository.Saved.Add(HealthInsurance.Create("Swiss Medical"));
        var request = new RenameHealthInsuranceRequest("  Swiss Medical  ");

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.RenameAsync(osde.Id, request, CancellationToken.None));
    }

    [Fact]
    public async Task RenameAsync_WithSameNameDifferentCase_UpdatesName()
    {
        var osde = HealthInsurance.Create("OSDE");
        _repository.Saved.Add(osde);
        var request = new RenameHealthInsuranceRequest("Osde");

        var response = await _service.RenameAsync(osde.Id, request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal("Osde", response.Name);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllHealthInsurances()
    {
        _repository.Saved.Add(HealthInsurance.Create("OSDE"));
        _repository.Saved.Add(HealthInsurance.Create("Swiss Medical"));

        var response = await _service.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, response.Count);
        Assert.Contains(response, h => h.Name == "OSDE");
        Assert.Contains(response, h => h.Name == "Swiss Medical");
    }

    [Fact]
    public async Task GetByIdAsync_WhenHealthInsuranceExists_ReturnsHealthInsurance()
    {
        var osde = HealthInsurance.Create("OSDE");
        _repository.Saved.Add(osde);

        var response = await _service.GetByIdAsync(osde.Id, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(osde.Id, response.Id);
        Assert.Equal("OSDE", response.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenHealthInsuranceDoesNotExist_ReturnsNull()
    {
        var response = await _service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public async Task DeleteAsync_WhenHealthInsuranceExists_RemovesItAndReturnsTrue()
    {
        var osde = HealthInsurance.Create("OSDE");
        _repository.Saved.Add(osde);

        var deleted = await _service.DeleteAsync(osde.Id, CancellationToken.None);

        Assert.True(deleted);
        Assert.Empty(_repository.Saved);
    }

    [Fact]
    public async Task DeleteAsync_WhenHealthInsuranceDoesNotExist_ReturnsFalse()
    {
        var deleted = await _service.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(deleted);
    }
}
