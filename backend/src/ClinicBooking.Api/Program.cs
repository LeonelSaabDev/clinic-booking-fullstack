using ClinicBooking.Api.Auth;
using ClinicBooking.Api.Endpoints;
using ClinicBooking.Api.ErrorHandling;
using ClinicBooking.Application;
using ClinicBooking.Application.Users;
using ClinicBooking.Domain.Users;
using ClinicBooking.Infrastructure;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecurityTransformer>());

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<KnownExceptionHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddClinicAuth(builder.Configuration);

const string FrontendCorsPolicy = "Frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

// One-off command to create the first account, since only a receptionist can create the others:
// dotnet run --project backend/src/ClinicBooking.Api -- create-receptionist <email> <password>
if (args is ["create-receptionist", var email, var password])
{
    using var scope = app.Services.CreateScope();
    var users = scope.ServiceProvider.GetRequiredService<UserService>();
    var created = await users.CreateAsync(
        new CreateUserRequest(email, password, UserRole.Receptionist, DoctorId: null), CancellationToken.None);
    Console.WriteLine($"Receptionist '{created.Email}' created.");
    return;
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapSpecialtyEndpoints();
app.MapHealthInsuranceEndpoints();
app.MapDoctorEndpoints();
app.MapPatientEndpoints();
app.MapAppointmentEndpoints();

app.Run();
