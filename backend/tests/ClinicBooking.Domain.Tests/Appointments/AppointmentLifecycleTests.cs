using ClinicBooking.Domain.Appointments;
using ClinicBooking.Domain.Common;
using ClinicBooking.Domain.Doctors;
using ClinicBooking.Domain.Patients;

namespace ClinicBooking.Domain.Tests.Appointments;

public class AppointmentLifecycleTests
{
    private static readonly DateTimeOffset StartsAt = new(2026, 10, 21, 9, 0, 0, TimeSpan.FromHours(-3));

    private static Appointment NewAppointment()
    {
        var doctor = Doctor.Create("Ana", "García", "MN 1", Guid.CreateVersion7());
        var patient = Patient.Create("Juan", "Pérez", "30123456", "juan@mail.com", "1155551234");
        return Appointment.Book(doctor, patient, StartsAt, useHealthInsurance: false);
    }

    [Fact]
    public void Cancel_WithEnoughNotice_CancelsTheAppointment()
    {
        var appointment = NewAppointment();

        appointment.Cancel(now: StartsAt - Appointment.MinimumCancellationNotice);

        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
    }

    [Fact]
    public void Cancel_WithLessThanTheMinimumNotice_ThrowsDomainException()
    {
        var appointment = NewAppointment();

        Assert.Throws<DomainException>(() =>
            appointment.Cancel(now: StartsAt - Appointment.MinimumCancellationNotice + TimeSpan.FromMinutes(1)));
        Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
    }

    [Fact]
    public void Complete_OnceTheAppointmentStarted_CompletesIt()
    {
        var appointment = NewAppointment();

        appointment.Complete(now: StartsAt.AddMinutes(10));

        Assert.Equal(AppointmentStatus.Completed, appointment.Status);
    }

    [Fact]
    public void MarkNoShow_OnceTheAppointmentStarted_MarksIt()
    {
        var appointment = NewAppointment();

        appointment.MarkNoShow(now: StartsAt.AddMinutes(20));

        Assert.Equal(AppointmentStatus.NoShow, appointment.Status);
    }

    [Fact]
    public void Complete_BeforeTheAppointmentStarts_ThrowsDomainException()
    {
        var appointment = NewAppointment();

        Assert.Throws<DomainException>(() => appointment.Complete(now: StartsAt.AddMinutes(-1)));
    }

    [Fact]
    public void MarkNoShow_BeforeTheAppointmentStarts_ThrowsDomainException()
    {
        var appointment = NewAppointment();

        Assert.Throws<DomainException>(() => appointment.MarkNoShow(now: StartsAt.AddMinutes(-1)));
    }

    public static TheoryData<Action<Appointment>> ClosingTransitions => new()
    {
        a => a.Cancel(StartsAt.AddDays(-2)),
        a => a.Complete(StartsAt.AddMinutes(5)),
        a => a.MarkNoShow(StartsAt.AddMinutes(5)),
    };

    [Theory]
    [MemberData(nameof(ClosingTransitions))]
    public void AnyTransition_FromAClosedAppointment_ThrowsDomainException(Action<Appointment> close)
    {
        var appointment = NewAppointment();
        close(appointment);
        var closedStatus = appointment.Status;

        Assert.Throws<DomainException>(() => appointment.Cancel(StartsAt.AddDays(-2)));
        Assert.Throws<DomainException>(() => appointment.Complete(StartsAt.AddMinutes(5)));
        Assert.Throws<DomainException>(() => appointment.MarkNoShow(StartsAt.AddMinutes(5)));
        Assert.Equal(closedStatus, appointment.Status);
    }
}
