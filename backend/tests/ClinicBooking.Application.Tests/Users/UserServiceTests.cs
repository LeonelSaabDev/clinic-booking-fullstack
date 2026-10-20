using ClinicBooking.Application.Common;
using ClinicBooking.Application.Tests.Doctors;
using ClinicBooking.Application.Users;
using ClinicBooking.Domain.Doctors;
using ClinicBooking.Domain.Users;

namespace ClinicBooking.Application.Tests.Users;

public class UserServiceTests
{
    private const string StrongPassword = "correct horse battery";

    private readonly FakeUserRepository _users = new();
    private readonly FakeDoctorRepository _doctors = new();
    private readonly UserService _service;
    private readonly Doctor _ana = Doctor.Create("Ana", "García", "MN 1", Guid.CreateVersion7());

    public UserServiceTests()
    {
        _doctors.Saved.Add(_ana);
        _service = new UserService(_users, _doctors, new FakePasswordHasher());
    }

    [Fact]
    public async Task CreateAsync_Receptionist_StoresOnlyTheHashedPassword()
    {
        var response = await _service.CreateAsync(
            new CreateUserRequest("recepcion@clinica.com", StrongPassword, UserRole.Receptionist, null),
            CancellationToken.None);

        var saved = Assert.Single(_users.Saved);
        Assert.Equal(response.Id, saved.Id);
        Assert.Equal($"hashed:{StrongPassword}", saved.PasswordHash);
        Assert.Equal(UserRole.Receptionist, response.Role);
    }

    [Fact]
    public async Task CreateAsync_DoctorUser_LinksTheDoctor()
    {
        var response = await _service.CreateAsync(
            new CreateUserRequest("ana@clinica.com", StrongPassword, UserRole.Doctor, _ana.Id), CancellationToken.None);

        Assert.Equal(_ana.Id, response.DoctorId);
    }

    [Fact]
    public async Task CreateAsync_WithShortPassword_ThrowsRequestValidationException()
    {
        await Assert.ThrowsAsync<RequestValidationException>(() => _service.CreateAsync(
            new CreateUserRequest("recepcion@clinica.com", "short", UserRole.Receptionist, null), CancellationToken.None));
        Assert.Empty(_users.Saved);
    }

    [Fact]
    public async Task CreateAsync_WithEmailAlreadyInUse_ThrowsConflictException()
    {
        _users.Saved.Add(User.CreateReceptionist("recepcion@clinica.com", "hash"));

        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(
            new CreateUserRequest(" RECEPCION@clinica.com", StrongPassword, UserRole.Receptionist, null),
            CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_DoctorUserForUnknownDoctor_ThrowsInvalidReferenceException()
    {
        await Assert.ThrowsAsync<InvalidReferenceException>(() => _service.CreateAsync(
            new CreateUserRequest("x@clinica.com", StrongPassword, UserRole.Doctor, Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_SecondUserForTheSameDoctor_ThrowsConflictException()
    {
        _users.Saved.Add(User.CreateDoctorUser("ana@clinica.com", "hash", _ana.Id));

        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(
            new CreateUserRequest("ana2@clinica.com", StrongPassword, UserRole.Doctor, _ana.Id), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_DoctorUserWithoutDoctor_ThrowsRequestValidationException()
    {
        await Assert.ThrowsAsync<RequestValidationException>(() => _service.CreateAsync(
            new CreateUserRequest("ana@clinica.com", StrongPassword, UserRole.Doctor, null), CancellationToken.None));
    }
}
