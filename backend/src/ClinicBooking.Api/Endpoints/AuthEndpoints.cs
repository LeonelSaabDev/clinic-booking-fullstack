using ClinicBooking.Api.Auth;
using ClinicBooking.Application.Auth;
using ClinicBooking.Application.Users;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ClinicBooking.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", Login).WithTags("Auth").AllowAnonymous();

        var users = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization(Policies.Receptionist);
        users.MapGet("/", GetAllUsers);
        users.MapPost("/", CreateUser);

        return app;
    }

    private static async Task<Ok<LoginResponse>> Login(
        LoginRequest request, AuthService service, CancellationToken cancellationToken) =>
        TypedResults.Ok(await service.LoginAsync(request, cancellationToken));

    private static async Task<Ok<IReadOnlyList<UserResponse>>> GetAllUsers(
        UserService service, CancellationToken cancellationToken) =>
        TypedResults.Ok(await service.GetAllAsync(cancellationToken));

    private static async Task<Created<UserResponse>> CreateUser(
        CreateUserRequest request, UserService service, CancellationToken cancellationToken)
    {
        var user = await service.CreateAsync(request, cancellationToken);
        return TypedResults.Created($"/api/users/{user.Id}", user);
    }
}
