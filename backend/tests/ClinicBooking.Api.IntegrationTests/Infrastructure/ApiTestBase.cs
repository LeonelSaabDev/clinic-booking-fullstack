using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClinicBooking.Application.Appointments;
using ClinicBooking.Application.Auth;
using ClinicBooking.Application.Doctors;
using ClinicBooking.Application.Patients;
using ClinicBooking.Application.Specialties;
using ClinicBooking.Application.Users;
using ClinicBooking.Domain.Users;

namespace ClinicBooking.Api.IntegrationTests.Infrastructure;

/// <summary>Shared helpers: sign in, and create the records a scenario needs through the real HTTP API.</summary>
[Collection(ClinicApiCollection.Name)]
public abstract class ApiTestBase(ClinicApiFactory factory)
{
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>A day a week from now, in clinic time, so every slot is in the future and can be cancelled.</summary>
    protected static readonly DateOnly BookingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

    private static readonly TimeSpan ClinicOffset = TimeSpan.FromHours(-3);

    protected ClinicApiFactory Factory { get; } = factory;

    /// <summary>Unique per call, so tests sharing the database never collide.</summary>
    protected static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}"[..Math.Min(prefix.Length + 9, 20)];

    protected static DateTimeOffset At(int hour, int minute = 0) =>
        new(BookingDay.ToDateTime(new TimeOnly(hour, minute)), ClinicOffset);

    protected HttpClient AnonymousClient() => Factory.CreateClient();

    protected async Task<HttpClient> SignedInClient(string email, string password)
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password), Json);
        response.EnsureSuccessStatusCode();
        var session = await response.Content.ReadFromJsonAsync<LoginResponse>(Json);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session!.AccessToken);
        return client;
    }

    protected Task<HttpClient> ReceptionistClient() =>
        SignedInClient(ClinicApiFactory.ReceptionistEmail, ClinicApiFactory.ReceptionistPassword);

    protected static async Task<T> PostAsync<T>(HttpClient client, string url, object body)
    {
        var response = await client.PostAsJsonAsync(url, body, Json);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"POST {url} -> {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return (await response.Content.ReadFromJsonAsync<T>(Json))!;
    }

    /// <summary>A doctor who works 09:00–12:00 on <see cref="BookingDay"/> with 30-minute appointments.</summary>
    protected static async Task<DoctorResponse> CreateDoctorAsync(HttpClient receptionist)
    {
        var specialty = await PostAsync<SpecialtyResponse>(receptionist, "/api/specialties",
            new CreateSpecialtyRequest(Unique("Specialty")));
        var doctor = await PostAsync<DoctorResponse>(receptionist, "/api/doctors",
            new CreateDoctorRequest("Ana", "Test", Unique("MN"), specialty.Id, []));

        var schedule = new UpdateDoctorScheduleRequest(30,
            [new ScheduleBlockDto(BookingDay.DayOfWeek, new TimeOnly(9, 0), new TimeOnly(12, 0))]);
        (await receptionist.PutAsJsonAsync($"/api/doctors/{doctor.Id}/schedule", schedule, Json)).EnsureSuccessStatusCode();

        return doctor;
    }

    protected static Task<PatientResponse> CreatePatientAsync(HttpClient receptionist)
    {
        var documentNumber = Random.Shared.NextInt64(10_000_000, 99_999_999).ToString();
        return PostAsync<PatientResponse>(receptionist, "/api/patients",
            new CreatePatientRequest("Juan", "Test", documentNumber, $"{documentNumber}@test.clinic", "1100000000", null, null));
    }

    protected static Task<AppointmentResponse> BookAsync(HttpClient client, Guid doctorId, Guid patientId, DateTimeOffset startsAt) =>
        PostAsync<AppointmentResponse>(client, "/api/appointments",
            new BookAppointmentRequest(doctorId, patientId, startsAt, UseHealthInsurance: false));

    protected static Task<HttpResponseMessage> TryBookAsync(HttpClient client, Guid doctorId, Guid patientId, DateTimeOffset startsAt) =>
        client.PostAsJsonAsync("/api/appointments",
            new BookAppointmentRequest(doctorId, patientId, startsAt, UseHealthInsurance: false), Json);

    /// <summary>Creates a doctor account through the API and returns a client signed in as that doctor.</summary>
    protected async Task<HttpClient> DoctorClientAsync(HttpClient receptionist, Guid doctorId)
    {
        var email = $"{Unique("doc")}@test.clinic";
        const string password = "doctor-test-passphrase";
        await PostAsync<UserResponse>(receptionist, "/api/users", new CreateUserRequest(email, password, UserRole.Doctor, doctorId));
        return await SignedInClient(email, password);
    }
}
