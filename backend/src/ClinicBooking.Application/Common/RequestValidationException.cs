namespace ClinicBooking.Application.Common;

/// <summary>The request is well formed but asks for something the use case cannot do, e.g. a time that is not a slot.</summary>
public sealed class RequestValidationException(string message) : Exception(message);
