namespace ClinicBooking.Infrastructure.Auth;

/// <summary>Claim names shared by the token issuer and the API that reads them.</summary>
public static class AuthClaims
{
    public const string Role = "role";
    public const string DoctorId = "doctor_id";
    public const string Email = "email";
}
