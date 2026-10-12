namespace ClinicBooking.Domain.Common;

public sealed class DomainException(string message) : Exception(message);
