using ClinicBooking.Domain.Users;

namespace ClinicBooking.Application.Users;

public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken);

    /// <param name="email">Already normalized (trimmed and lowercase).</param>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> ExistsByDoctorAsync(Guid doctorId, CancellationToken cancellationToken);
    void Add(User user);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
