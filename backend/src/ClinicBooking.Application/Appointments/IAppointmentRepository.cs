using ClinicBooking.Domain.Appointments;

namespace ClinicBooking.Application.Appointments;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Appointments starting in [<paramref name="from"/>, <paramref name="to"/>), ordered by start.</summary>
    Task<IReadOnlyList<Appointment>> ListAsync(DateTimeOffset from, DateTimeOffset to, Guid? doctorId, CancellationToken cancellationToken);

    /// <summary>Start instants of the doctor's scheduled appointments in [<paramref name="from"/>, <paramref name="to"/>).</summary>
    Task<IReadOnlyList<DateTimeOffset>> GetScheduledStartsAsync(Guid doctorId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);

    Task<bool> IsDoctorBookedAtAsync(Guid doctorId, DateTimeOffset startsAt, CancellationToken cancellationToken);
    Task<bool> PatientHasScheduledOverlapAsync(Guid patientId, DateTimeOffset startsAt, DateTimeOffset endsAt, CancellationToken cancellationToken);
    Task<bool> ExistsByDoctorAsync(Guid doctorId, CancellationToken cancellationToken);
    Task<bool> ExistsByPatientAsync(Guid patientId, CancellationToken cancellationToken);
    void Add(Appointment appointment);

    /// <summary>Throws <see cref="Common.ConflictException"/> when the database rejects a double booking.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
