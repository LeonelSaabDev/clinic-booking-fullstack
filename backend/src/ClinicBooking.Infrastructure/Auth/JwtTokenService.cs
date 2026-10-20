using System.Security.Claims;
using ClinicBooking.Application.Auth;
using ClinicBooking.Domain.Users;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ClinicBooking.Infrastructure.Auth;

internal sealed class JwtTokenService(JwtOptions options, TimeProvider timeProvider) : ITokenService
{
    private readonly SigningCredentials _credentials = new(options.CreateSigningKey(), SecurityAlgorithms.HmacSha256);
    private readonly JsonWebTokenHandler _handler = new();

    public AccessToken CreateToken(User user)
    {
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(options.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(AuthClaims.Role, user.Role.ToString()),
        };
        if (user.DoctorId is { } doctorId)
            claims.Add(new Claim(AuthClaims.DoctorId, doctorId.ToString()));

        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = options.Issuer,
            Audience = options.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = _credentials,
        });

        return new AccessToken(token, expiresAt);
    }
}
