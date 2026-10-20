using ClinicBooking.Application.Auth;
using ClinicBooking.Application.Common;
using ClinicBooking.Domain.Users;

namespace ClinicBooking.Application.Tests.Users;

public class AuthServiceTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _users.Saved.Add(User.CreateReceptionist("recepcion@clinica.com", _hasher.Hash("correct horse battery")));
        _service = new AuthService(_users, _hasher, new FakeTokenService());
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokenAndRole()
    {
        var response = await _service.LoginAsync(
            new LoginRequest("  Recepcion@Clinica.com ", "correct horse battery"), CancellationToken.None);

        Assert.Equal("token-for:recepcion@clinica.com", response.AccessToken);
        Assert.Equal(FakeTokenService.ExpiresAt, response.ExpiresAt);
        Assert.Equal(UserRole.Receptionist, response.Role);
        Assert.Null(response.DoctorId);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsAuthenticationFailedException()
    {
        await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
            _service.LoginAsync(new LoginRequest("recepcion@clinica.com", "wrong"), CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_FailsWithTheSameMessageAsAWrongPassword()
    {
        var unknown = await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
            _service.LoginAsync(new LoginRequest("nobody@clinica.com", "whatever"), CancellationToken.None));
        var wrongPassword = await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
            _service.LoginAsync(new LoginRequest("recepcion@clinica.com", "whatever"), CancellationToken.None));

        Assert.Equal(wrongPassword.Message, unknown.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public async Task LoginAsync_WithMalformedEmail_ThrowsAuthenticationFailedException(string email)
    {
        await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
            _service.LoginAsync(new LoginRequest(email, "whatever"), CancellationToken.None));
    }
}
