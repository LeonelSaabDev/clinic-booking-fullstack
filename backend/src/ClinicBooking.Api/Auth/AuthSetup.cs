using ClinicBooking.Domain.Users;
using ClinicBooking.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace ClinicBooking.Api.Auth;

public static class Policies
{
    /// <summary>Front desk: manages every record.</summary>
    public const string Receptionist = nameof(Receptionist);

    /// <summary>Anyone working at the clinic: receptionists and doctors.</summary>
    public const string Staff = nameof(Staff);
}

public static class AuthSetup
{
    public static IServiceCollection AddClinicAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        jwt.EnsureValid();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep claim names exactly as issued ("role", "email") instead of the legacy XML-style URIs.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = jwt.CreateSigningKey(),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    RoleClaimType = AuthClaims.Role,
                    NameClaimType = AuthClaims.Email,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.Receptionist, policy => policy.RequireRole(nameof(UserRole.Receptionist)))
            .AddPolicy(Policies.Staff, policy => policy.RequireRole(nameof(UserRole.Receptionist), nameof(UserRole.Doctor)))
            // Secure by default: an endpoint someone forgets to protect still requires a signed-in user.
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        return services;
    }
}
