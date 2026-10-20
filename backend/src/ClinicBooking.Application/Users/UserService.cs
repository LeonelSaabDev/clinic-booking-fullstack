using ClinicBooking.Application.Auth;
using ClinicBooking.Application.Common;
using ClinicBooking.Application.Doctors;
using ClinicBooking.Domain.Common;
using ClinicBooking.Domain.Users;

namespace ClinicBooking.Application.Users;

public sealed record CreateUserRequest(string Email, string Password, UserRole Role, Guid? DoctorId);

public sealed record UserResponse(Guid Id, string Email, UserRole Role, Guid? DoctorId)
{
    public static UserResponse FromEntity(User user) => new(user.Id, user.Email, user.Role, user.DoctorId);
}

public sealed class UserService(IUserRepository users, IDoctorRepository doctors, IPasswordHasher passwordHasher)
{
    /// <summary>Length matters more than symbol rules; a long passphrase is both stronger and easier to remember.</summary>
    public const int MinPasswordLength = 10;
    public const int MaxPasswordLength = 128;

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var all = await users.GetAllAsync(cancellationToken);
        return all.Select(UserResponse.FromEntity).ToList();
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var password = request.Password ?? string.Empty;
        if (password.Length is < MinPasswordLength or > MaxPasswordLength)
            throw new RequestValidationException(
                $"The password must have between {MinPasswordLength} and {MaxPasswordLength} characters.");

        var email = EmailAddress.Normalize(request.Email, "User email");
        if (await users.ExistsByEmailAsync(email, cancellationToken))
            throw new ConflictException($"A user with email '{email}' already exists.");

        var passwordHash = passwordHasher.Hash(password);
        var user = request.Role switch
        {
            UserRole.Receptionist => User.CreateReceptionist(email, passwordHash),
            UserRole.Doctor => await CreateDoctorUserAsync(email, passwordHash, request.DoctorId, cancellationToken),
            _ => throw new RequestValidationException("Unknown role."),
        };

        users.Add(user);
        await users.SaveChangesAsync(cancellationToken);

        return UserResponse.FromEntity(user);
    }

    private async Task<User> CreateDoctorUserAsync(
        string email, string passwordHash, Guid? doctorId, CancellationToken cancellationToken)
    {
        if (doctorId is not { } id)
            throw new RequestValidationException("A doctor user must be linked to a doctor.");

        if (await doctors.GetByIdAsync(id, cancellationToken) is null)
            throw new InvalidReferenceException("The selected doctor does not exist.");

        if (await users.ExistsByDoctorAsync(id, cancellationToken))
            throw new ConflictException("This doctor already has a user account.");

        return User.CreateDoctorUser(email, passwordHash, id);
    }
}
