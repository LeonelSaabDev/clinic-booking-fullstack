using ClinicBooking.Domain.Common;

namespace ClinicBooking.Domain.Doctors;

public sealed class Doctor
{
    public const int NameMaxLength = 100;
    public const int LicenseNumberMaxLength = 20;
    public const int DefaultAppointmentDurationMinutes = 30;
    public const int MinAppointmentDurationMinutes = 10;
    public const int MaxAppointmentDurationMinutes = 120;
    public const int AppointmentDurationStepMinutes = 5;

    private readonly List<AcceptedHealthInsurance> _acceptedHealthInsurances = [];
    private readonly List<ScheduleBlock> _weeklySchedule = [];

    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;
    public Guid SpecialtyId { get; private set; }

    public int AppointmentDurationMinutes { get; private set; } = DefaultAppointmentDurationMinutes;

    public IReadOnlyCollection<AcceptedHealthInsurance> AcceptedHealthInsurances => _acceptedHealthInsurances.AsReadOnly();
    public IReadOnlyCollection<ScheduleBlock> WeeklySchedule => _weeklySchedule.AsReadOnly();

    private Doctor() { }

    public static Doctor Create(string firstName, string lastName, string licenseNumber, Guid specialtyId)
    {
        var doctor = new Doctor { Id = Guid.CreateVersion7() };
        doctor.UpdateDetails(firstName, lastName, licenseNumber, specialtyId);
        return doctor;
    }

    public void UpdateDetails(string firstName, string lastName, string licenseNumber, Guid specialtyId)
    {
        var trimmedFirstName = Required(firstName, "Doctor first name", NameMaxLength);
        var trimmedLastName = Required(lastName, "Doctor last name", NameMaxLength);
        var trimmedLicenseNumber = Required(licenseNumber, "Doctor license number", LicenseNumberMaxLength);

        if (specialtyId == Guid.Empty)
            throw new DomainException("Doctor specialty is required.");

        FirstName = trimmedFirstName;
        LastName = trimmedLastName;
        LicenseNumber = trimmedLicenseNumber;
        SpecialtyId = specialtyId;
    }

    public void SetAcceptedHealthInsurances(IEnumerable<Guid> healthInsuranceIds)
    {
        var distinctIds = healthInsuranceIds.Distinct().ToList();

        if (distinctIds.Contains(Guid.Empty))
            throw new DomainException("Accepted health insurance id cannot be empty.");

        _acceptedHealthInsurances.Clear();
        _acceptedHealthInsurances.AddRange(distinctIds.Select(id => new AcceptedHealthInsurance(id)));
    }

    public bool Accepts(Guid healthInsuranceId) =>
        _acceptedHealthInsurances.Any(a => a.HealthInsuranceId == healthInsuranceId);

    public void SetSchedule(int appointmentDurationMinutes, IEnumerable<ScheduleBlock> blocks)
    {
        if (appointmentDurationMinutes is < MinAppointmentDurationMinutes or > MaxAppointmentDurationMinutes)
            throw new DomainException(
                $"Appointment duration must be between {MinAppointmentDurationMinutes} and {MaxAppointmentDurationMinutes} minutes.");

        if (appointmentDurationMinutes % AppointmentDurationStepMinutes != 0)
            throw new DomainException($"Appointment duration must be a multiple of {AppointmentDurationStepMinutes} minutes.");

        var duration = TimeSpan.FromMinutes(appointmentDurationMinutes);
        var newBlocks = blocks.ToList();

        if (newBlocks.Any(block => block.Length < duration))
            throw new DomainException("Every schedule block must be at least as long as one appointment.");

        for (var i = 0; i < newBlocks.Count; i++)
        {
            for (var j = i + 1; j < newBlocks.Count; j++)
            {
                if (newBlocks[i].Overlaps(newBlocks[j]))
                    throw new DomainException($"Schedule blocks on {newBlocks[i].Day} cannot overlap.");
            }
        }

        AppointmentDurationMinutes = appointmentDurationMinutes;
        _weeklySchedule.Clear();
        _weeklySchedule.AddRange(newBlocks);
    }

    public IReadOnlyList<TimeOnly> GetSlotStarts(DayOfWeek day)
    {
        var duration = TimeSpan.FromMinutes(AppointmentDurationMinutes);
        var slots = new List<TimeOnly>();

        // Persisted data bypasses SetSchedule; a non-positive duration would make the loop below endless.
        if (duration <= TimeSpan.Zero)
            return slots;

        foreach (var block in _weeklySchedule.Where(b => b.Day == day).OrderBy(b => b.Start))
        {
            // TimeSpan arithmetic instead of TimeOnly.Add, which silently wraps around midnight.
            for (var cursor = block.Start.ToTimeSpan(); cursor + duration <= block.End.ToTimeSpan(); cursor += duration)
                slots.Add(TimeOnly.FromTimeSpan(cursor));
        }

        return slots;
    }

    private static string Required(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{fieldName} is required.");

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > maxLength)
            throw new DomainException($"{fieldName} cannot exceed {maxLength} characters.");

        return trimmedValue;
    }
}
