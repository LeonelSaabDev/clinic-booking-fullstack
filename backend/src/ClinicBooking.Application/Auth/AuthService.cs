using ClinicBooking.Application.Common;
using ClinicBooking.Application.Users;
using ClinicBooking.Domain.Common;
using ClinicBooking.Domain.Users;

namespace ClinicBooking.Application.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, string Email, UserRole Role, Guid? DoctorId);

public sealed class AuthService(IUserRepository users, IPasswordHasher passwordHasher, ITokenService tokenService)
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await FindUserAsync(request.Email, cancellationToken);

        // Same failure for "no such user" and "wrong password", so the response never reveals which emails exist.
        if (user is null || !passwordHasher.Verify(user.PasswordHash, request.Password ?? string.Empty))
            throw new AuthenticationFailedException();

        var token = tokenService.CreateToken(user);
        return new LoginResponse(token.Token, token.ExpiresAt, user.Email, user.Role, user.DoctorId);
    }

    private async Task<User?> FindUserAsync(string email, CancellationToken cancellationToken)
    {
        try
        {
            return await users.GetByEmailAsync(EmailAddress.Normalize(email, "Email"), cancellationToken);
        }
        catch (DomainException)
        {
            return null;
        }
    }
}
