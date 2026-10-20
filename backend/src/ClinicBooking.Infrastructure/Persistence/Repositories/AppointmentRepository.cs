using ClinicBooking.Application.Appointments;
using ClinicBooking.Application.Common;
using ClinicBooking.Domain.Appointments;
using ClinicBooking.Infrastructure.Persistence.Configurations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Infrastructure.Persistence.Repositories;

internal sealed class AppointmentRepository(AppDbContext context) : IAppointmentRepository
{
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    public Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Appointments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Appointment>> ListAsync(
        DateTimeOffset from, DateTimeOffset to, Guid? doctorId, CancellationToken cancellationToken) =>
        await context.Appointments
            .AsNoTracking()
            .Where(a => a.StartsAt >= from && a.StartsAt < to && (doctorId == null || a.DoctorId == doctorId))
            .OrderBy(a => a.StartsAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DateTimeOffset>> GetScheduledStartsAsync(
        Guid doctorId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) =>
        await context.Appointments
            .Where(a => a.DoctorId == doctorId && a.Status == AppointmentStatus.Scheduled &&
                        a.StartsAt >= from && a.StartsAt < to)
            .Select(a => a.StartsAt)
            .ToListAsync(cancellationToken);

    public Task<bool> IsDoctorBookedAtAsync(Guid doctorId, DateTimeOffset startsAt, CancellationToken cancellationToken) =>
        context.Appointments.AnyAsync(
            a => a.DoctorId == doctorId && a.Status == AppointmentStatus.Scheduled && a.StartsAt == startsAt,
            cancellationToken);

    public Task<bool> PatientHasScheduledOverlapAsync(
        Guid patientId, DateTimeOffset startsAt, DateTimeOffset endsAt, CancellationToken cancellationToken) =>
        context.Appointments.AnyAsync(
            a => a.PatientId == patientId && a.Status == AppointmentStatus.Scheduled &&
                 a.StartsAt < endsAt && startsAt < a.EndsAt,
            cancellationToken);

    public Task<bool> ExistsByDoctorAsync(Guid doctorId, CancellationToken cancellationToken) =>
        context.Appointments.AnyAsync(a => a.DoctorId == doctorId, cancellationToken);

    public Task<bool> ExistsByPatientAsync(Guid patientId, CancellationToken cancellationToken) =>
        context.Appointments.AnyAsync(a => a.PatientId == patientId, cancellationToken);

    public void Add(Appointment appointment) => context.Appointments.Add(appointment);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDoubleBooking(exception))
        {
            throw new ConflictException("That slot was just booked by someone else. Please choose another one.");
        }
    }

    private static bool IsDoubleBooking(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: UniqueIndexViolation or UniqueConstraintViolation } sql &&
        sql.Message.Contains(AppointmentConfiguration.DoctorSlotIndexName, StringComparison.Ordinal);
}
