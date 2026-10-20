using ClinicBooking.Domain.Users;

namespace ClinicBooking.Application.Auth;

/// <summary>Implemented in Infrastructure with ASP.NET Core Identity's PBKDF2 hasher.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string passwordHash, string password);
}

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);

/// <summary>Implemented in Infrastructure as a signed JWT.</summary>
public interface ITokenService
{
    AccessToken CreateToken(User user);
}
