namespace ClinicBooking.Application.Common;

/// <summary>Sign-in failed. The message never says whether the email or the password was wrong.</summary>
public sealed class AuthenticationFailedException() : Exception("Invalid email or password.");
