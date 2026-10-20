using ClinicBooking.Application.Appointments;
using ClinicBooking.Domain.Appointments;

namespace ClinicBooking.Application.Tests.Appointments;

internal sealed class FakeAppointmentRepository : IAppointmentRepository
{
    private readonly List<Appointment> _pending = [];

    public List<Appointment> Saved { get; } = [];

    public Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.FirstOrDefault(a => a.Id == id));

    public Task<IReadOnlyList<Appointment>> ListAsync(
        DateTimeOffset from, DateTimeOffset to, Guid? doctorId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Appointment>>(Saved
            .Where(a => a.StartsAt >= from && a.StartsAt < to && (doctorId == null || a.DoctorId == doctorId))
            .OrderBy(a => a.StartsAt)
            .ToList());

    public Task<IReadOnlyList<DateTimeOffset>> GetScheduledStartsAsync(
        Guid doctorId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<DateTimeOffset>>(Saved
            .Where(a => a.Status == AppointmentStatus.Scheduled && a.DoctorId == doctorId &&
                        a.StartsAt >= from && a.StartsAt < to)
            .Select(a => a.StartsAt)
            .ToList());

    public Task<bool> IsDoctorBookedAtAsync(Guid doctorId, DateTimeOffset startsAt, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Any(a =>
            a.Status == AppointmentStatus.Scheduled && a.DoctorId == doctorId && a.StartsAt == startsAt));

    public Task<bool> PatientHasScheduledOverlapAsync(
        Guid patientId, DateTimeOffset startsAt, DateTimeOffset endsAt, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Any(a =>
            a.Status == AppointmentStatus.Scheduled && a.PatientId == patientId && a.Overlaps(startsAt, endsAt)));

    public Task<bool> ExistsByDoctorAsync(Guid doctorId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Any(a => a.DoctorId == doctorId));

    public Task<bool> ExistsByPatientAsync(Guid patientId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Any(a => a.PatientId == patientId));

    public void Add(Appointment appointment) => _pending.Add(appointment);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        return Task.CompletedTask;
    }
}
