using ClinicBooking.Api.Auth;
using ClinicBooking.Application.Patients;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ClinicBooking.Api.Endpoints;

public static class PatientEndpoints
{
    public static IEndpointRouteBuilder MapPatientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/patients").WithTags("Patients").RequireAuthorization(Policies.Staff);

        group.MapGet("/", SearchPatients);
        group.MapGet("/{id:guid}", GetPatientById).WithName(nameof(GetPatientById));
        group.MapPost("/", CreatePatient).RequireAuthorization(Policies.Receptionist);
        group.MapPut("/{id:guid}", UpdatePatient).RequireAuthorization(Policies.Receptionist);
        group.MapDelete("/{id:guid}", DeletePatient).RequireAuthorization(Policies.Receptionist);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<PatientResponse>>> SearchPatients(
        string? search, PatientService service, CancellationToken cancellationToken)
    {
        var patients = await service.SearchAsync(search, cancellationToken);
        return TypedResults.Ok(patients);
    }

    private static async Task<Results<Ok<PatientResponse>, NotFound>> GetPatientById(
        Guid id, PatientService service, CancellationToken cancellationToken)
    {
        var patient = await service.GetByIdAsync(id, cancellationToken);
        return patient is null ? TypedResults.NotFound() : TypedResults.Ok(patient);
    }

    private static async Task<CreatedAtRoute<PatientResponse>> CreatePatient(
        CreatePatientRequest request, PatientService service, CancellationToken cancellationToken)
    {
        var patient = await service.CreateAsync(request, cancellationToken);
        return TypedResults.CreatedAtRoute(patient, nameof(GetPatientById), new { id = patient.Id });
    }

    private static async Task<Results<Ok<PatientResponse>, NotFound>> UpdatePatient(
        Guid id, UpdatePatientRequest request, PatientService service, CancellationToken cancellationToken)
    {
        var patient = await service.UpdateAsync(id, request, cancellationToken);
        return patient is null ? TypedResults.NotFound() : TypedResults.Ok(patient);
    }

    private static async Task<Results<NoContent, NotFound>> DeletePatient(
        Guid id, PatientService service, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
