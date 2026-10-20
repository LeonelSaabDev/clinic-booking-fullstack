using ClinicBooking.Application.Appointments;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ClinicBooking.Api.Endpoints;

public static class AppointmentEndpoints
{
    public static IEndpointRouteBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments").WithTags("Appointments");

        group.MapGet("/", ListAppointments);
        group.MapGet("/{id:guid}", GetAppointmentById).WithName(nameof(GetAppointmentById));
        group.MapPost("/", BookAppointment);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<AppointmentResponse>>> ListAppointments(
        DateOnly date, Guid? doctorId, AppointmentService service, CancellationToken cancellationToken)
    {
        var appointments = await service.ListAsync(date, doctorId, cancellationToken);
        return TypedResults.Ok(appointments);
    }

    private static async Task<Results<Ok<AppointmentResponse>, NotFound>> GetAppointmentById(
        Guid id, AppointmentService service, CancellationToken cancellationToken)
    {
        var appointment = await service.GetByIdAsync(id, cancellationToken);
        return appointment is null ? TypedResults.NotFound() : TypedResults.Ok(appointment);
    }

    private static async Task<CreatedAtRoute<AppointmentResponse>> BookAppointment(
        BookAppointmentRequest request, AppointmentService service, CancellationToken cancellationToken)
    {
        var appointment = await service.BookAsync(request, cancellationToken);
        return TypedResults.CreatedAtRoute(appointment, nameof(GetAppointmentById), new { id = appointment.Id });
    }
}
