using ClinicBooking.Application.Common;
using ClinicBooking.Application.Doctors;
using ClinicBooking.Domain.Specialties;

namespace ClinicBooking.Application.Specialties;

public sealed class SpecialtyService(ISpecialtyRepository repository, IDoctorRepository doctors)
{
    public async Task<IReadOnlyList<SpecialtyResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var specialties = await repository.GetAllAsync(cancellationToken);
        return specialties.Select(SpecialtyResponse.FromEntity).ToList();
    }

    public async Task<SpecialtyResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var specialty = await repository.GetByIdAsync(id, cancellationToken);
        return specialty is null ? null : SpecialtyResponse.FromEntity(specialty);
    }

    public async Task<SpecialtyResponse> CreateAsync(CreateSpecialtyRequest request, CancellationToken cancellationToken)
    {
        var specialty = Specialty.Create(request.Name);

        if (await repository.ExistsByNameAsync(specialty.Name, excludedId: null, cancellationToken))
            throw new ConflictException($"A specialty named '{specialty.Name}' already exists.");

        repository.Add(specialty);
        await repository.SaveChangesAsync(cancellationToken);

        return SpecialtyResponse.FromEntity(specialty);
    }

    public async Task<SpecialtyResponse?> RenameAsync(Guid id, RenameSpecialtyRequest request, CancellationToken cancellationToken)
    {
        var specialty = await repository.GetByIdAsync(id, cancellationToken);
        if (specialty is null)
            return null;

        specialty.Rename(request.Name);

        if (await repository.ExistsByNameAsync(specialty.Name, excludedId: specialty.Id, cancellationToken))
            throw new ConflictException($"A specialty named '{specialty.Name}' already exists.");

        await repository.SaveChangesAsync(cancellationToken);

        return SpecialtyResponse.FromEntity(specialty);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var specialty = await repository.GetByIdAsync(id, cancellationToken);
        if (specialty is null)
            return false;

        if (await doctors.ExistsBySpecialtyAsync(specialty.Id, cancellationToken))
            throw new ConflictException($"'{specialty.Name}' cannot be deleted because it has doctors assigned.");

        repository.Remove(specialty);
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
