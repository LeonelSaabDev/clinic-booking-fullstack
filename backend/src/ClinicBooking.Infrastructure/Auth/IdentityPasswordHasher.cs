using ClinicBooking.Application.Auth;
using ClinicBooking.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace ClinicBooking.Infrastructure.Auth;

/// <summary>
/// Adapts ASP.NET Core Identity's hasher (PBKDF2 with a random salt and many iterations) to our own interface,
/// so the application never depends on Identity and the crypto is not reinvented.
/// </summary>
internal sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(user: null!, password);

    public bool Verify(string passwordHash, string password) =>
        _hasher.VerifyHashedPassword(user: null!, passwordHash, password) != PasswordVerificationResult.Failed;
}
