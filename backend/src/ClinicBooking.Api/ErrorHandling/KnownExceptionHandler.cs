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
            InvalidReferenceException => (StatusCodes.Status400BadRequest, "Invalid reference"),
            RequestValidationException => (StatusCodes.Status400BadRequest, "Validation error"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            AuthenticationFailedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
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
