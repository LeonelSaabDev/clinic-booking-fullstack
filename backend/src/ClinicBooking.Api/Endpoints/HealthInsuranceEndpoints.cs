using ClinicBooking.Application.HealthInsurances;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ClinicBooking.Api.Endpoints;

public static class HealthInsuranceEndpoints
{
    public static IEndpointRouteBuilder MapHealthInsuranceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/health-insurances").WithTags("Health Insurances");

        group.MapGet("/", GetAllHealthInsurances);
        group.MapGet("/{id:guid}", GetHealthInsuranceById).WithName(nameof(GetHealthInsuranceById));
        group.MapPost("/", CreateHealthInsurance);
        group.MapPut("/{id:guid}", RenameHealthInsurance);
        group.MapDelete("/{id:guid}", DeleteHealthInsurance);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<HealthInsuranceResponse>>> GetAllHealthInsurances(
        HealthInsuranceService service, CancellationToken cancellationToken)
    {
        var healthInsurances = await service.GetAllAsync(cancellationToken);
        return TypedResults.Ok(healthInsurances);
    }

    private static async Task<Results<Ok<HealthInsuranceResponse>, NotFound>> GetHealthInsuranceById(
        Guid id, HealthInsuranceService service, CancellationToken cancellationToken)
    {
        var healthInsurance = await service.GetByIdAsync(id, cancellationToken);
        return healthInsurance is null ? TypedResults.NotFound() : TypedResults.Ok(healthInsurance);
    }

    private static async Task<CreatedAtRoute<HealthInsuranceResponse>> CreateHealthInsurance(
        CreateHealthInsuranceRequest request, HealthInsuranceService service, CancellationToken cancellationToken)
    {
        var healthInsurance = await service.CreateAsync(request, cancellationToken);
        return TypedResults.CreatedAtRoute(healthInsurance, nameof(GetHealthInsuranceById), new { id = healthInsurance.Id });
    }

    private static async Task<Results<Ok<HealthInsuranceResponse>, NotFound>> RenameHealthInsurance(
        Guid id, RenameHealthInsuranceRequest request, HealthInsuranceService service, CancellationToken cancellationToken)
    {
        var healthInsurance = await service.RenameAsync(id, request, cancellationToken);
        return healthInsurance is null ? TypedResults.NotFound() : TypedResults.Ok(healthInsurance);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteHealthInsurance(
        Guid id, HealthInsuranceService service, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
