using ClinicBooking.Application.Common;
using ClinicBooking.Domain.Specialties;

namespace ClinicBooking.Application.Specialties;

public sealed class SpecialtyService(ISpecialtyRepository repository)
{
    public async Task<SpecialtyResponse> CreateAsync(CreateSpecialtyRequest request, CancellationToken cancellationToken)
    {
        var specialty = Specialty.Create(request.Name);

        if (await repository.ExistsByNameAsync(specialty.Name, excludedId: null, cancellationToken))
            throw new ConflictException($"A specialty named '{specialty.Name}' already exists.");

        repository.Add(specialty);
        await repository.SaveChangesAsync(cancellationToken);

        return SpecialtyResponse.FromEntity(specialty);
    }
}
