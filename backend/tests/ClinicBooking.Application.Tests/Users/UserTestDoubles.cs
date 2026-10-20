using ClinicBooking.Application.Auth;
using ClinicBooking.Application.Users;
using ClinicBooking.Domain.Users;

namespace ClinicBooking.Application.Tests.Users;

internal sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> _pending = [];

    public List<User> Saved { get; } = [];

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<User>>(Saved.ToList());

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.FirstOrDefault(u => u.Email == email));

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Any(u => u.Email == email));

    public Task<bool> ExistsByDoctorAsync(Guid doctorId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Any(u => u.DoctorId == doctorId));

    public void Add(User user) => _pending.Add(user);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        return Task.CompletedTask;
    }
}

/// <summary>Readable stand-in for the real hasher: "secret" becomes "hashed:secret".</summary>
internal sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"hashed:{password}";

    public bool Verify(string passwordHash, string password) => passwordHash == Hash(password);
}

internal sealed class FakeTokenService : ITokenService
{
    public static readonly DateTimeOffset ExpiresAt = new(2026, 10, 21, 10, 0, 0, TimeSpan.Zero);

    public AccessToken CreateToken(User user) => new($"token-for:{user.Email}", ExpiresAt);
}
