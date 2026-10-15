using ClinicBooking.Domain.HealthInsurances;

namespace ClinicBooking.Application.HealthInsurances;

public sealed record CreateHealthInsuranceRequest(string Name);

public sealed record RenameHealthInsuranceRequest(string Name);

public sealed record HealthInsuranceResponse(Guid Id, string Name)
{
    public static HealthInsuranceResponse FromEntity(HealthInsurance healthInsurance) =>
        new(healthInsurance.Id, healthInsurance.Name);
}
