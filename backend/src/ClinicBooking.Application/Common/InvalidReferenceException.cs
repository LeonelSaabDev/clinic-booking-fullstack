namespace ClinicBooking.Application.Common;

public sealed class InvalidReferenceException(string message) : Exception(message);
