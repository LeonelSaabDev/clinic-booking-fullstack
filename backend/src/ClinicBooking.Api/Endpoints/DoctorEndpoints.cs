using ClinicBooking.Api.Auth;
using ClinicBooking.Application.Availability;
using ClinicBooking.Application.Doctors;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ClinicBooking.Api.Endpoints;

public static class DoctorEndpoints
{
    public static IEndpointRouteBuilder MapDoctorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/doctors").WithTags("Doctors").RequireAuthorization(Policies.Staff);

        group.MapGet("/", GetAllDoctors);
        group.MapGet("/{id:guid}", GetDoctorById).WithName(nameof(GetDoctorById));
        group.MapPost("/", CreateDoctor).RequireAuthorization(Policies.Receptionist);
        group.MapPut("/{id:guid}", UpdateDoctor).RequireAuthorization(Policies.Receptionist);
        group.MapDelete("/{id:guid}", DeleteDoctor).RequireAuthorization(Policies.Receptionist);

        group.MapGet("/{id:guid}/schedule", GetDoctorSchedule);
        group.MapPut("/{id:guid}/schedule", UpdateDoctorSchedule).RequireAuthorization(Policies.Receptionist);
        group.MapGet("/{id:guid}/availability", GetDoctorAvailability);

        return app;
    }

    private static async Task<Results<Ok<DoctorScheduleResponse>, NotFound>> GetDoctorSchedule(
        Guid id, DoctorService service, CancellationToken cancellationToken)
    {
        var schedule = await service.GetScheduleAsync(id, cancellationToken);
        return schedule is null ? TypedResults.NotFound() : TypedResults.Ok(schedule);
    }

    private static async Task<Results<Ok<DoctorScheduleResponse>, NotFound>> UpdateDoctorSchedule(
        Guid id, UpdateDoctorScheduleRequest request, DoctorService service, CancellationToken cancellationToken)
    {
        var schedule = await service.UpdateScheduleAsync(id, request, cancellationToken);
        return schedule is null ? TypedResults.NotFound() : TypedResults.Ok(schedule);
    }

    private static async Task<Results<Ok<IReadOnlyList<AvailableSlotResponse>>, NotFound>> GetDoctorAvailability(
        Guid id, DateOnly date, AvailabilityService service, CancellationToken cancellationToken)
    {
        var slots = await service.GetAvailableSlotsAsync(id, date, cancellationToken);
        return slots is null ? TypedResults.NotFound() : TypedResults.Ok(slots);
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
