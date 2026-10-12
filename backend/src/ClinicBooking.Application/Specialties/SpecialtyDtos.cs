using ClinicBooking.Domain.Specialties;

namespace ClinicBooking.Application.Specialties;

public sealed record CreateSpecialtyRequest(string Name);

public sealed record RenameSpecialtyRequest(string Name);

public sealed record SpecialtyResponse(Guid Id, string Name)
{
    public static SpecialtyResponse FromEntity(Specialty specialty) =>
        new(specialty.Id, specialty.Name);
}
