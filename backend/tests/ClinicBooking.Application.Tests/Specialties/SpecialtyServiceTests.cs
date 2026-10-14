using ClinicBooking.Application.Common;
using ClinicBooking.Application.Specialties;
using ClinicBooking.Domain.Specialties;

namespace ClinicBooking.Application.Tests.Specialties;

public class SpecialtyServiceTests
{
    private readonly FakeSpecialtyRepository _repository = new();

    private readonly SpecialtyService _service;

    public SpecialtyServiceTests()
    {
        _service = new SpecialtyService(_repository);
    }

    [Fact]
    public async Task CreateAsync_WithUniqueName_SavesAndReturnsSpecialty()
    {
        var request = new CreateSpecialtyRequest("Cardiology");

        var response = await _service.CreateAsync(request, CancellationToken.None);

        Assert.Equal("Cardiology", response.Name);
        var saved = Assert.Single(_repository.Saved);
        Assert.Equal(response.Id, saved.Id);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ThrowsConflictException()
    {
        _repository.Saved.Add(Specialty.Create("Cardiology"));
        var request = new CreateSpecialtyRequest("Cardiology");

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateNameAndExtraSpaces_ThrowsConflictException()
    {
        _repository.Saved.Add(Specialty.Create("Cardiology"));
        var request = new CreateSpecialtyRequest("  Cardiology  ");

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(request, CancellationToken.None));
    }
}
