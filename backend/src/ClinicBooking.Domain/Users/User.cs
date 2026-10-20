using ClinicBooking.Domain.Common;

namespace ClinicBooking.Domain.Users;

/// <summary>Someone who can sign in. Hashing passwords is infrastructure, so the entity only stores the hash.</summary>
public sealed class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }

    /// <summary>Set only for doctors: the doctor record whose agenda this user can see.</summary>
    public Guid? DoctorId { get; private set; }

    private User() { }

    public static User CreateReceptionist(string email, string passwordHash) =>
        Create(email, passwordHash, UserRole.Receptionist, doctorId: null);

    public static User CreateDoctorUser(string email, string passwordHash, Guid doctorId)
    {
        if (doctorId == Guid.Empty)
            throw new DomainException("A doctor user must be linked to a doctor.");

        return Create(email, passwordHash, UserRole.Doctor, doctorId);
    }

    public void ChangePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        PasswordHash = passwordHash;
    }

    private static User Create(string email, string passwordHash, UserRole role, Guid? doctorId)
    {
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = EmailAddress.Normalize(email, "User email"),
            Role = role,
            DoctorId = doctorId,
        };
        user.ChangePasswordHash(passwordHash);
        return user;
    }
}
