using ClinicBooking.Domain.Common;

namespace ClinicBooking.Domain.Specialties;

public sealed class Specialty
{
    public const int NameMaxLength = 100;

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Specialty() { }

    public static Specialty Create(string name)
    {
        var specialty = new Specialty { Id = Guid.CreateVersion7() };
        specialty.Rename(name);
        return specialty;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Specialty name is required.");

        var trimmedName = name.Trim();

        if (trimmedName.Length > NameMaxLength)
            throw new DomainException($"Specialty name cannot exceed {NameMaxLength} characters.");
        
        Name = trimmedName;
    }
}
