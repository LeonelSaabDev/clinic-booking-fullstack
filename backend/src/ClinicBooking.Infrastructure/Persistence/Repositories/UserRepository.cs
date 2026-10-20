using ClinicBooking.Application.Users;
using ClinicBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.Users.AsNoTracking().OrderBy(u => u.Email).ToListAsync(cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken) =>
        context.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public Task<bool> ExistsByDoctorAsync(Guid doctorId, CancellationToken cancellationToken) =>
        context.Users.AnyAsync(u => u.DoctorId == doctorId, cancellationToken);

    public void Add(User user) => context.Users.Add(user);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
