using ClinicBooking.Application.Doctors;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ClinicBooking.Api.Endpoints;

public static class DoctorEndpoints
{
    public static IEndpointRouteBuilder MapDoctorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/doctors").WithTags("Doctors");

        group.MapGet("/", GetAllDoctors);
        group.MapGet("/{id:guid}", GetDoctorById).WithName(nameof(GetDoctorById));
        group.MapPost("/", CreateDoctor);
        group.MapPut("/{id:guid}", UpdateDoctor);
        group.MapDelete("/{id:guid}", DeleteDoctor);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<DoctorResponse>>> GetAllDoctors(
        DoctorService service, CancellationToken cancellationToken)
    {
        var doctors = await service.GetAllAsync(cancellationToken);
        return TypedResults.Ok(doctors);
    }

    private static async Task<Results<Ok<DoctorResponse>, NotFound>> GetDoctorById(
        Guid id, DoctorService service, CancellationToken cancellationToken)
    {
        var doctor = await service.GetByIdAsync(id, cancellationToken);
        return doctor is null ? TypedResults.NotFound() : TypedResults.Ok(doctor);
    }

    private static async Task<CreatedAtRoute<DoctorResponse>> CreateDoctor(
        CreateDoctorRequest request, DoctorService service, CancellationToken cancellationToken)
    {
        var doctor = await service.CreateAsync(request, cancellationToken);
        return TypedResults.CreatedAtRoute(doctor, nameof(GetDoctorById), new { id = doctor.Id });
    }

    private static async Task<Results<Ok<DoctorResponse>, NotFound>> UpdateDoctor(
        Guid id, UpdateDoctorRequest request, DoctorService service, CancellationToken cancellationToken)
    {
        var doctor = await service.UpdateAsync(id, request, cancellationToken);
        return doctor is null ? TypedResults.NotFound() : TypedResults.Ok(doctor);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteDoctor(
        Guid id, DoctorService service, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
