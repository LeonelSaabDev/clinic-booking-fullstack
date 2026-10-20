using ClinicBooking.Application.Common;
using ClinicBooking.Application.Doctors;
using ClinicBooking.Domain.HealthInsurances;

namespace ClinicBooking.Application.HealthInsurances;

public sealed class HealthInsuranceService(IHealthInsuranceRepository repository, IDoctorRepository doctors)
{
    public async Task<IReadOnlyList<HealthInsuranceResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var healthInsurances = await repository.GetAllAsync(cancellationToken);
        return healthInsurances.Select(HealthInsuranceResponse.FromEntity).ToList();
    }

    public async Task<HealthInsuranceResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var healthInsurance = await repository.GetByIdAsync(id, cancellationToken);
        return healthInsurance is null ? null : HealthInsuranceResponse.FromEntity(healthInsurance);
    }

    public async Task<HealthInsuranceResponse> CreateAsync(CreateHealthInsuranceRequest request, CancellationToken cancellationToken)
    {
        var healthInsurance = HealthInsurance.Create(request.Name);

        if (await repository.ExistsByNameAsync(healthInsurance.Name, excludedId: null, cancellationToken))
            throw new ConflictException($"A health insurance named '{healthInsurance.Name}' already exists.");

        repository.Add(healthInsurance);
        await repository.SaveChangesAsync(cancellationToken);

        return HealthInsuranceResponse.FromEntity(healthInsurance);
    }

    public async Task<HealthInsuranceResponse?> RenameAsync(Guid id, RenameHealthInsuranceRequest request, CancellationToken cancellationToken)
    {
        var healthInsurance = await repository.GetByIdAsync(id, cancellationToken);
        if (healthInsurance is null)
            return null;

        healthInsurance.Rename(request.Name);

        if (await repository.ExistsByNameAsync(healthInsurance.Name, excludedId: healthInsurance.Id, cancellationToken))
            throw new ConflictException($"A health insurance named '{healthInsurance.Name}' already exists.");

        await repository.SaveChangesAsync(cancellationToken);

        return HealthInsuranceResponse.FromEntity(healthInsurance);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var healthInsurance = await repository.GetByIdAsync(id, cancellationToken);
        if (healthInsurance is null)
            return false;

        if (await doctors.ExistsByHealthInsuranceAsync(healthInsurance.Id, cancellationToken))
            throw new ConflictException($"'{healthInsurance.Name}' cannot be deleted because doctors accept it.");

        repository.Remove(healthInsurance);
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
