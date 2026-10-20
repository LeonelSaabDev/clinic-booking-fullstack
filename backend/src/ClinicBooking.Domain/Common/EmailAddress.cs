using System.Net.Mail;

namespace ClinicBooking.Domain.Common;

public static class EmailAddress
{
    public const int MaxLength = 254;

    /// <summary>Trims, lowercases and validates an email so it can be compared and stored consistently.</summary>
    public static string Normalize(string email, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException($"{fieldName} is required.");

        var normalized = email.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
            throw new DomainException($"{fieldName} cannot exceed {MaxLength} characters.");

        // MailAddress accepts "user@localhost", which is technically valid but useless for real contact.
        if (!MailAddress.TryCreate(normalized, out var address) || address.Address != normalized ||
            !address.Host.Contains('.'))
            throw new DomainException($"{fieldName} is not valid.");

        return normalized;
    }
}
