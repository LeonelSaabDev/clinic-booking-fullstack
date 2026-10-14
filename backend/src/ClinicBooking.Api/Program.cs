using ClinicBooking.Application;
using ClinicBooking.Infrastructure;
using ClinicBooking.Api.Endpoints;
using Scalar.AspNetCore;
using ClinicBooking.Api.ErrorHandling;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<KnownExceptionHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapSpecialtyEndpoints();

app.Run();

