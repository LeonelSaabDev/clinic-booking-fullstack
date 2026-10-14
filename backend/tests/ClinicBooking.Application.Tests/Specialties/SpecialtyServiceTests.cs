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

    [Fact]
    public async Task RenameAsync_WithNewUniqueName_UpdatesAndReturnsSpecialty()
    {
        var specialty = Specialty.Create("Cardiology");
        _repository.Saved.Add(specialty);
        var request = new RenameSpecialtyRequest("Pediatrics");

        var response = await _service.RenameAsync(specialty.Id, request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal("Pediatrics", response.Name);
        Assert.Equal("Pediatrics", specialty.Name);
    }

    [Fact]
    public async Task RenameAsync_WhenSpecialtyDoesNotExist_ReturnsNull()
    {
        var request = new RenameSpecialtyRequest("Pediatrics");

        var response = await _service.RenameAsync(Guid.NewGuid(), request, CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public async Task RenameAsync_WithNameOfAnotherSpecialty_ThrowsConflictException()
    {
        var cardiology = Specialty.Create("Cardiology");
        _repository.Saved.Add(cardiology);
        _repository.Saved.Add(Specialty.Create("Pediatrics"));
        var request = new RenameSpecialtyRequest("Pediatrics");

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.RenameAsync(cardiology.Id, request, CancellationToken.None));
    }

    [Fact]
    public async Task RenameAsync_WithSameNameDifferentCase_UpdatesName()
    {
        var cardiology = Specialty.Create("Cardiology");
        _repository.Saved.Add(cardiology);
        var request = new RenameSpecialtyRequest("cardiology");

        var response = await _service.RenameAsync(cardiology.Id, request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal("cardiology", response.Name);
    }
}
