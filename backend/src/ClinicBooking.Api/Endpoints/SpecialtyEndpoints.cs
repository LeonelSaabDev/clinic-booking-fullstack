using ClinicBooking.Application.Specialties;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ClinicBooking.Api.Endpoints;

public static class SpecialtyEndpoints
{
    public static IEndpointRouteBuilder MapSpecialtyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/specialties").WithTags("Specialties");

        group.MapGet("/", GetAllSpecialties);
        group.MapGet("/{id:guid}", GetSpecialtyById).WithName(nameof(GetSpecialtyById));
        group.MapPost("/", CreateSpecialty);
        group.MapPut("/{id:guid}", RenameSpecialty);
        group.MapDelete("/{id:guid}", DeleteSpecialty);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<SpecialtyResponse>>> GetAllSpecialties(
        SpecialtyService service, CancellationToken cancellationToken)
    {
        var specialties = await service.GetAllAsync(cancellationToken);
        return TypedResults.Ok(specialties);
    }

    private static async Task<Results<Ok<SpecialtyResponse>, NotFound>> GetSpecialtyById(
        Guid id, SpecialtyService service, CancellationToken cancellationToken)
    {
        var specialty = await service.GetByIdAsync(id, cancellationToken);
        return specialty is null ? TypedResults.NotFound() : TypedResults.Ok(specialty);
    }

    private static async Task<CreatedAtRoute<SpecialtyResponse>> CreateSpecialty(
        CreateSpecialtyRequest request, SpecialtyService service, CancellationToken cancellationToken)
    {
        var specialty = await service.CreateAsync(request, cancellationToken);
        return TypedResults.CreatedAtRoute(specialty, nameof(GetSpecialtyById), new { id = specialty.Id });
    }

    private static async Task<Results<Ok<SpecialtyResponse>, NotFound>> RenameSpecialty(
        Guid id, RenameSpecialtyRequest request, SpecialtyService service, CancellationToken cancellationToken)
    {
        var specialty = await service.RenameAsync(id, request, cancellationToken);
        return specialty is null ? TypedResults.NotFound() : TypedResults.Ok(specialty);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteSpecialty(
        Guid id, SpecialtyService service, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
