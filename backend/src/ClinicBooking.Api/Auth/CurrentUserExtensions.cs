using System.Security.Claims;
using ClinicBooking.Domain.Users;
using ClinicBooking.Infrastructure.Auth;

namespace ClinicBooking.Api.Auth;

public static class CurrentUserExtensions
{
    /// <summary>
    /// The doctor record a doctor user is linked to, read from the signed token, never from the request.
    /// Null for receptionists, who may act on any doctor.
    /// </summary>
    public static Guid? GetActingDoctorId(this ClaimsPrincipal user)
    {
        if (!user.IsInRole(nameof(UserRole.Doctor)))
            return null;

        return Guid.TryParse(user.FindFirstValue(AuthClaims.DoctorId), out var doctorId)
            ? doctorId
            : throw new InvalidOperationException("A doctor token must carry a doctor_id claim.");
    }
}
