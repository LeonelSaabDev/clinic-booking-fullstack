using ClinicBooking.Domain.Common;

namespace ClinicBooking.Domain.HealthInsurances;

public sealed class HealthInsurance
{
    public const int NameMaxLength = 100;

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private HealthInsurance() { }

    public static HealthInsurance Create(string name)
    {
        var healthInsurance = new HealthInsurance { Id = Guid.CreateVersion7() };
        healthInsurance.Rename(name);
        return healthInsurance;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Health insurance name is required.");

        var trimmedName = name.Trim();

        if (trimmedName.Length > NameMaxLength)
            throw new DomainException($"Health insurance name cannot exceed {NameMaxLength} characters.");

        Name = trimmedName;
    }
}
