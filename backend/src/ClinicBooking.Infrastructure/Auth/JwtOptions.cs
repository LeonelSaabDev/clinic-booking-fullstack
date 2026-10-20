using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ClinicBooking.Infrastructure.Auth;

/// <summary>Bound from the "Jwt" section. The signing key comes from User Secrets, never from the repository.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>HMAC-SHA256 needs a key of at least 256 bits.</summary>
    public const int MinSigningKeyBytes = 32;

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SigningKey { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 60;

    public SymmetricSecurityKey CreateSigningKey() => new(Encoding.UTF8.GetBytes(SigningKey));

    /// <summary>Fails fast at startup with an actionable message instead of at the first sign-in.</summary>
    public void EnsureValid()
    {
        if (Encoding.UTF8.GetByteCount(SigningKey) < MinSigningKeyBytes)
            throw new InvalidOperationException(
                $"'Jwt:SigningKey' must be at least {MinSigningKeyBytes} bytes. For development, set it with: " +
                "dotnet user-secrets set \"Jwt:SigningKey\" \"<a long random value>\" --project backend/src/ClinicBooking.Api");

        if (string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("'Jwt:Issuer' and 'Jwt:Audience' are required.");

        if (ExpirationMinutes <= 0)
            throw new InvalidOperationException("'Jwt:ExpirationMinutes' must be positive.");
    }
}
