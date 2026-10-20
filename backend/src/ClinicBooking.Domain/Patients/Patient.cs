using ClinicBooking.Domain.Common;

namespace ClinicBooking.Domain.Patients;

public sealed class Patient
{
    public const int NameMaxLength = 100;
    public const int DocumentNumberMinLength = 6;
    public const int DocumentNumberMaxLength = 10;
    public const int EmailMaxLength = EmailAddress.MaxLength;
    public const int PhoneMaxLength = 30;
    public const int MemberNumberMaxLength = 30;

    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string DocumentNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public Guid? HealthInsuranceId { get; private set; }
    public string? MemberNumber { get; private set; }

    private Patient() { }

    public static Patient Create(string firstName, string lastName, string documentNumber, string email, string phone)
    {
        var patient = new Patient { Id = Guid.CreateVersion7() };
        patient.UpdateDetails(firstName, lastName, documentNumber, email, phone);
        return patient;
    }

    public void UpdateDetails(string firstName, string lastName, string documentNumber, string email, string phone)
    {
        var trimmedFirstName = Required(firstName, "Patient first name", NameMaxLength);
        var trimmedLastName = Required(lastName, "Patient last name", NameMaxLength);
        var normalizedDocument = NormalizeDocumentNumber(documentNumber);
        var normalizedEmail = EmailAddress.Normalize(email, "Patient email");
        var trimmedPhone = NormalizePhone(phone);

        FirstName = trimmedFirstName;
        LastName = trimmedLastName;
        DocumentNumber = normalizedDocument;
        Email = normalizedEmail;
        Phone = trimmedPhone;
    }

    /// <summary>Sets the patient's health insurance, or clears it when <paramref name="healthInsuranceId"/> is null.</summary>
    public void SetCoverage(Guid? healthInsuranceId, string? memberNumber)
    {
        if (healthInsuranceId is null)
        {
            HealthInsuranceId = null;
            MemberNumber = null;
            return;
        }

        if (healthInsuranceId == Guid.Empty)
            throw new DomainException("Patient health insurance id cannot be empty.");

        var trimmedMemberNumber = Required(memberNumber ?? string.Empty, "Member number", MemberNumberMaxLength);

        HealthInsuranceId = healthInsuranceId;
        MemberNumber = trimmedMemberNumber;
    }

    /// <summary>Accepts the usual ways a DNI is typed ("30.123.456", "30 123 456") and keeps only the digits.</summary>
    private static string NormalizeDocumentNumber(string documentNumber)
    {
        var compact = Required(documentNumber, "Patient document number", int.MaxValue)
            .Replace(".", string.Empty)
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty);

        if (!compact.All(char.IsAsciiDigit) ||
            compact.Length is < DocumentNumberMinLength or > DocumentNumberMaxLength)
            throw new DomainException(
                $"Patient document number must have between {DocumentNumberMinLength} and {DocumentNumberMaxLength} digits.");

        return compact;
    }

    private static string NormalizePhone(string phone)
    {
        var trimmedPhone = Required(phone, "Patient phone", PhoneMaxLength);

        if (!trimmedPhone.All(c => char.IsAsciiDigit(c) || c is ' ' or '-' or '+' or '(' or ')'))
            throw new DomainException("Patient phone can only contain digits, spaces and + - ( ).");

        return trimmedPhone;
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
