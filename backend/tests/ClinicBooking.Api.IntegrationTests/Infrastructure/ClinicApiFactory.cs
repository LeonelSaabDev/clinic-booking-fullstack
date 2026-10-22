using ClinicBooking.Application.Users;
using ClinicBooking.Domain.Users;
using ClinicBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace ClinicBooking.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Hosts the real API in memory against a throwaway SQL Server running in Docker.
/// One container is shared by every test in the collection; each test creates its own data.
/// </summary>
public sealed class ClinicApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string ReceptionistEmail = "recepcion@test.clinic";
    public const string ReceptionistPassword = "integration-test-passphrase";

    private readonly MsSqlContainer _sqlServer =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" skips appsettings.Development.json and User Secrets, so nothing from the developer's machine leaks in.
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:ClinicBookingDb", ConnectionString);
        builder.UseSetting("Jwt:SigningKey", "integration-tests-signing-key-that-is-long-enough");
    }

    private string ConnectionString =>
        new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(_sqlServer.GetConnectionString())
        {
            InitialCatalog = "ClinicBookingTests",
        }.ConnectionString;

    public async Task InitializeAsync()
    {
        await _sqlServer.StartAsync();

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<UserService>().CreateAsync(
            new CreateUserRequest(ReceptionistEmail, ReceptionistPassword, UserRole.Receptionist, DoctorId: null),
            CancellationToken.None);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _sqlServer.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class ClinicApiCollection : ICollectionFixture<ClinicApiFactory>
{
    public const string Name = "Clinic API";
}
