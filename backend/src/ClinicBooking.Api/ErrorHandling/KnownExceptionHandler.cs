using ClinicBooking.Application.Common;
using ClinicBooking.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace ClinicBooking.Api.ErrorHandling;

internal sealed class KnownExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        (int Status, string Title)? error = exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Validation error"),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Invalid request"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => null
        };

        if (error is null)
            return false;

        httpContext.Response.StatusCode = error.Value.Status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Title = error.Value.Title,
                Detail = exception.Message,
                Status = error.Value.Status
            }
        });
    }
}
