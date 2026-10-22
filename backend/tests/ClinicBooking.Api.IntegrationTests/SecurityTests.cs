using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ClinicBooking.Api.IntegrationTests.Infrastructure;
using ClinicBooking.Application.Appointments;
using ClinicBooking.Application.Auth;
using ClinicBooking.Application.Specialties;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBooking.Api.IntegrationTests;

public class SecurityTests(ClinicApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var response = await AnonymousClient().GetAsync("/api/doctors");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPasswordOrUnknownEmail_ReturnsTheSame401()
    {
        var client = AnonymousClient();

        var wrongPassword = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(ClinicApiFactory.ReceptionistEmail, "not the password"), Json);
        var unknownEmail = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("nobody@test.clinic", "not the password"), Json);

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmail.StatusCode);
        var first = await wrongPassword.Content.ReadFromJsonAsync<ProblemDetails>(Json);
        var second = await unknownEmail.Content.ReadFromJsonAsync<ProblemDetails>(Json);
        Assert.Equal(first!.Detail, second!.Detail);
    }

    [Fact]
    public async Task TamperedToken_Returns401()
    {
        var receptionist = await ReceptionistClient();
        var token = receptionist.DefaultRequestHeaders.Authorization!.Parameter!;
        var tampered = AnonymousClient();
        tampered.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token[..^4] + "AAAA");

        var response = await tampered.GetAsync("/api/doctors");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Doctor_CannotManageRecords_Returns403()
    {
        var receptionist = await ReceptionistClient();
        var doctor = await CreateDoctorAsync(receptionist);
        var doctorClient = await DoctorClientAsync(receptionist, doctor.Id);

        var createSpecialty = await doctorClient.PostAsJsonAsync("/api/specialties", new CreateSpecialtyRequest("Hack"), Json);
        var listUsers = await doctorClient.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, createSpecialty.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, listUsers.StatusCode);
    }

    [Fact]
    public async Task Doctor_AskingForAnotherDoctorsAgenda_GetsOnlyTheirOwn()
    {
        var receptionist = await ReceptionistClient();
        var ana = await CreateDoctorAsync(receptionist);
        var luis = await CreateDoctorAsync(receptionist);
        var patient = await CreatePatientAsync(receptionist);
        var anasAppointment = await BookAsync(receptionist, ana.Id, patient.Id, At(9));
        var luisAppointment = await BookAsync(receptionist, luis.Id, patient.Id, At(10));
        var anaClient = await DoctorClientAsync(receptionist, ana.Id);

        var agenda = await anaClient.GetFromJsonAsync<List<AppointmentResponse>>(
            $"/api/appointments?date={BookingDay:yyyy-MM-dd}&doctorId={luis.Id}", Json);
        var luisAppointmentById = await anaClient.GetAsync($"/api/appointments/{luisAppointment.Id}");
        var completeLuisAppointment = await anaClient.PostAsync($"/api/appointments/{luisAppointment.Id}/complete", null);

        Assert.Equal([anasAppointment.Id], agenda!.Select(a => a.Id));
        Assert.Equal(HttpStatusCode.NotFound, luisAppointmentById.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, completeLuisAppointment.StatusCode);
    }
}
