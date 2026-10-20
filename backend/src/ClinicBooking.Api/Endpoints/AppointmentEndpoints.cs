using ClinicBooking.Api.Auth;
using ClinicBooking.Application.Appointments;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ClinicBooking.Api.Endpoints;

public static class AppointmentEndpoints
{
    public static IEndpointRouteBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments").WithTags("Appointments").RequireAuthorization(Policies.Staff);

        group.MapGet("/", ListAppointments);
        group.MapGet("/{id:guid}", GetAppointmentById).WithName(nameof(GetAppointmentById));
        group.MapPost("/", BookAppointment).RequireAuthorization(Policies.Receptionist);

        // State transitions are commands with their own rules, not a free "PATCH status" field.
        group.MapPost("/{id:guid}/cancel", CancelAppointment).RequireAuthorization(Policies.Receptionist);
        group.MapPost("/{id:guid}/complete", CompleteAppointment);
        group.MapPost("/{id:guid}/no-show", MarkAppointmentAsNoShow);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<AppointmentResponse>>> ListAppointments(
        DateOnly date, Guid? doctorId, ClaimsPrincipal user, AppointmentService service, CancellationToken cancellationToken)
    {
        // A doctor always gets their own agenda, whatever doctorId the request asks for.
        var appointments = await service.ListAsync(date, user.GetActingDoctorId() ?? doctorId, cancellationToken);
        return TypedResults.Ok(appointments);
    }

    private static async Task<Results<Ok<AppointmentResponse>, NotFound>> GetAppointmentById(
        Guid id, ClaimsPrincipal user, AppointmentService service, CancellationToken cancellationToken)
    {
        var appointment = await service.GetByIdAsync(id, cancellationToken);
        var actingDoctorId = user.GetActingDoctorId();

        // Another doctor's appointment looks exactly like a missing one.
        return appointment is null || (actingDoctorId is not null && appointment.DoctorId != actingDoctorId)
            ? TypedResults.NotFound()
            : TypedResults.Ok(appointment);
    }

    private static async Task<CreatedAtRoute<AppointmentResponse>> BookAppointment(
        BookAppointmentRequest request, AppointmentService service, CancellationToken cancellationToken)
    {
        var appointment = await service.BookAsync(request, cancellationToken);
        return TypedResults.CreatedAtRoute(appointment, nameof(GetAppointmentById), new { id = appointment.Id });
    }

    private static async Task<Results<Ok<AppointmentResponse>, NotFound>> CancelAppointment(
        Guid id, AppointmentService service, CancellationToken cancellationToken) =>
        ToResult(await service.CancelAsync(id, cancellationToken));

    private static async Task<Results<Ok<AppointmentResponse>, NotFound>> CompleteAppointment(
        Guid id, ClaimsPrincipal user, AppointmentService service, CancellationToken cancellationToken) =>
        ToResult(await service.CompleteAsync(id, user.GetActingDoctorId(), cancellationToken));

    private static async Task<Results<Ok<AppointmentResponse>, NotFound>> MarkAppointmentAsNoShow(
        Guid id, ClaimsPrincipal user, AppointmentService service, CancellationToken cancellationToken) =>
        ToResult(await service.MarkNoShowAsync(id, user.GetActingDoctorId(), cancellationToken));

    private static Results<Ok<AppointmentResponse>, NotFound> ToResult(AppointmentResponse? appointment) =>
        appointment is null ? TypedResults.NotFound() : TypedResults.Ok(appointment);
}
