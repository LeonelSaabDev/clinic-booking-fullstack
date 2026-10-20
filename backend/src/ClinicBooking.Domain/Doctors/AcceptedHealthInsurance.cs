namespace ClinicBooking.Domain.Doctors;

public sealed class AcceptedHealthInsurance
{
    public Guid HealthInsuranceId { get; private set; }

    private AcceptedHealthInsurance() { }

    internal AcceptedHealthInsurance(Guid healthInsuranceId)
    {
        HealthInsuranceId = healthInsuranceId;
    }
}
