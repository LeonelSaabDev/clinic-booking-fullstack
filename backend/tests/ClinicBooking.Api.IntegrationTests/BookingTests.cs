using System.Net;
using System.Net.Http.Json;
using ClinicBooking.Api.IntegrationTests.Infrastructure;
using ClinicBooking.Application.Appointments;
using ClinicBooking.Application.Availability;
using ClinicBooking.Application.Common;
using ClinicBooking.Domain.Appointments;
using ClinicBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicBooking.Api.IntegrationTests;

public class BookingTests(ClinicApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Book_ReturnsTheAppointmentInClinicTime_AndRemovesTheSlotFromAvailability()
    {
        var receptionist = await ReceptionistClient();
        var doctor = await CreateDoctorAsync(receptionist);
        var patient = await CreatePatientAsync(receptionist);

        var appointment = await BookAsync(receptionist, doctor.Id, patient.Id, At(9));
        var slots = await receptionist.GetFromJsonAsync<List<AvailableSlotResponse>>(
            $"/api/doctors/{doctor.Id}/availability?date={BookingDay:yyyy-MM-dd}", Json);

        Assert.Equal(At(9), appointment.StartsAt);
        Assert.Equal(TimeSpan.FromHours(-3), appointment.StartsAt.Offset);
        Assert.DoesNotContain(slots!, s => s.StartsAt == At(9));
        Assert.Contains(slots!, s => s.StartsAt == At(9, 30));
    }

    [Fact]
    public async Task Book_SameSlotTwice_SecondGets409()
    {
        var receptionist = await ReceptionistClient();
        var doctor = await CreateDoctorAsync(receptionist);
        var first = await CreatePatientAsync(receptionist);
        var second = await CreatePatientAsync(receptionist);
        await BookAsync(receptionist, doctor.Id, first.Id, At(9));

        var response = await TryBookAsync(receptionist, doctor.Id, second.Id, At(9));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ConcurrentBookings_ForTheSameSlot_OnlyOneSucceeds()
    {
        const int racers = 20;
        var receptionist = await ReceptionistClient();
        var doctor = await CreateDoctorAsync(receptionist);
        var patients = new List<Guid>();
        for (var i = 0; i < racers; i++)
            patients.Add((await CreatePatientAsync(receptionist)).Id);

        var responses = await Task.WhenAll(patients.Select(patientId => TryBookAsync(receptionist, doctor.Id, patientId, At(10))));

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(racers - 1, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));
    }

    [Fact]
    public async Task UniqueIndex_RejectsASecondScheduledAppointment_EvenWhenTheServiceCheckIsBypassed()
    {
        var receptionist = await ReceptionistClient();
        var doctor = await CreateDoctorAsync(receptionist);
        var first = await CreatePatientAsync(receptionist);
        var second = await CreatePatientAsync(receptionist);
        await BookAsync(receptionist, doctor.Id, first.Id, At(11));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var doctorEntity = await db.Doctors.SingleAsync(d => d.Id == doctor.Id);
        var patientEntity = await db.Patients.SingleAsync(p => p.Id == second.Id);
        var repository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();

        // Simulates the race: a request that already passed the "is it free?" check writes the same slot.
        repository.Add(Appointment.Book(doctorEntity, patientEntity, At(11), useHealthInsurance: false));

        var conflict = await Assert.ThrowsAsync<ConflictException>(() => repository.SaveChangesAsync(CancellationToken.None));
        Assert.Contains("just booked by someone else", conflict.Message);
    }

    [Fact]
    public async Task CancelledSlot_CanBeBookedAgain()
    {
        var receptionist = await ReceptionistClient();
        var doctor = await CreateDoctorAsync(receptionist);
        var first = await CreatePatientAsync(receptionist);
        var second = await CreatePatientAsync(receptionist);
        var booked = await BookAsync(receptionist, doctor.Id, first.Id, At(9, 30));

        var cancel = await receptionist.PostAsync($"/api/appointments/{booked.Id}/cancel", null);
        var rebooked = await TryBookAsync(receptionist, doctor.Id, second.Id, At(9, 30));

        Assert.Equal(HttpStatusCode.OK, cancel.StatusCode);
        Assert.Equal(HttpStatusCode.Created, rebooked.StatusCode);
    }

    [Fact]
    public async Task Book_TimeThatIsNotASlot_Returns400()
    {
        var receptionist = await ReceptionistClient();
        var doctor = await CreateDoctorAsync(receptionist);
        var patient = await CreatePatientAsync(receptionist);

        var response = await TryBookAsync(receptionist, doctor.Id, patient.Id, At(9, 10));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
