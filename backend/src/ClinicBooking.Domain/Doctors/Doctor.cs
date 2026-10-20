using ClinicBooking.Domain.Common;

namespace ClinicBooking.Domain.Doctors;

public sealed class Doctor
{
    public const int NameMaxLength = 100;
    public const int LicenseNumberMaxLength = 20;

    private readonly List<AcceptedHealthInsurance> _acceptedHealthInsurances = [];

    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;
    public Guid SpecialtyId { get; private set; }

    public IReadOnlyCollection<AcceptedHealthInsurance> AcceptedHealthInsurances => _acceptedHealthInsurances.AsReadOnly();

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
