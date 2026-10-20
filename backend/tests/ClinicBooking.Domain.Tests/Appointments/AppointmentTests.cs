using ClinicBooking.Domain.Appointments;
using ClinicBooking.Domain.Common;
using ClinicBooking.Domain.Doctors;
using ClinicBooking.Domain.Patients;

namespace ClinicBooking.Domain.Tests.Appointments;

public class AppointmentTests
{
    private static readonly Guid Osde = Guid.CreateVersion7();
    private static readonly DateTimeOffset NineAmInArgentina = new(2026, 10, 21, 9, 0, 0, TimeSpan.FromHours(-3));

    private static Doctor DoctorAccepting(params Guid[] healthInsuranceIds)
    {
        var doctor = Doctor.Create("Ana", "García", "MN 12345", Guid.CreateVersion7());
        doctor.SetSchedule(45, []);
        doctor.SetAcceptedHealthInsurances(healthInsuranceIds);
        return doctor;
    }

    private static Patient PatientWith(Guid? healthInsuranceId)
    {
        var patient = Patient.Create("Juan", "Pérez", "30123456", "juan@mail.com", "1155551234");
        patient.SetCoverage(healthInsuranceId, healthInsuranceId is null ? null : "210-555");
        return patient;
    }

    [Fact]
    public void Book_AsPrivate_CreatesScheduledAppointmentLastingOneSlot()
    {
        var doctor = DoctorAccepting();
        var patient = PatientWith(null);

        var appointment = Appointment.Book(doctor, patient, NineAmInArgentina, useHealthInsurance: false);

        Assert.NotEqual(Guid.Empty, appointment.Id);
        Assert.Equal(doctor.Id, appointment.DoctorId);
        Assert.Equal(patient.Id, appointment.PatientId);
        Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
        Assert.Equal(NineAmInArgentina, appointment.StartsAt);
        Assert.Equal(NineAmInArgentina.AddMinutes(45), appointment.EndsAt);
        Assert.Null(appointment.HealthInsuranceId);
        Assert.Null(appointment.MemberNumber);
    }

    [Fact]
    public void Book_StoresInstantsInUtc()
    {
        var appointment = Appointment.Book(DoctorAccepting(), PatientWith(null), NineAmInArgentina, false);

        Assert.Equal(TimeSpan.Zero, appointment.StartsAt.Offset);
        Assert.Equal(12, appointment.StartsAt.Hour);
    }

    [Fact]
    public void Book_WithAcceptedHealthInsurance_CopiesThePatientCoverage()
    {
        var appointment = Appointment.Book(DoctorAccepting(Osde), PatientWith(Osde), NineAmInArgentina, true);

        Assert.Equal(Osde, appointment.HealthInsuranceId);
        Assert.Equal("210-555", appointment.MemberNumber);
    }

    [Fact]
    public void Book_KeepsItsCoverageWhenThePatientChangesHealthInsuranceLater()
    {
        var patient = PatientWith(Osde);
        var appointment = Appointment.Book(DoctorAccepting(Osde), patient, NineAmInArgentina, true);

        patient.SetCoverage(Guid.CreateVersion7(), "999");

        Assert.Equal(Osde, appointment.HealthInsuranceId);
        Assert.Equal("210-555", appointment.MemberNumber);
    }

    [Fact]
    public void Book_WithHealthInsuranceTheDoctorDoesNotAccept_ThrowsDomainException()
    {
        var doctor = DoctorAccepting(Guid.CreateVersion7());

        Assert.Throws<DomainException>(() => Appointment.Book(doctor, PatientWith(Osde), NineAmInArgentina, true));
    }

    [Fact]
    public void Book_WithHealthInsuranceForPatientWithoutOne_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Appointment.Book(DoctorAccepting(Osde), PatientWith(null), NineAmInArgentina, true));
    }

    [Fact]
    public void Book_AsPrivate_IgnoresThePatientHealthInsurance()
    {
        var appointment = Appointment.Book(DoctorAccepting(), PatientWith(Osde), NineAmInArgentina, false);

        Assert.Null(appointment.HealthInsuranceId);
    }

    [Fact]
    public void Overlaps_DetectsSharedTimeButNotTouchingAppointments()
    {
        var appointment = Appointment.Book(DoctorAccepting(), PatientWith(null), NineAmInArgentina, false);

        Assert.True(appointment.Overlaps(NineAmInArgentina.AddMinutes(30), NineAmInArgentina.AddMinutes(75)));
        Assert.False(appointment.Overlaps(NineAmInArgentina.AddMinutes(45), NineAmInArgentina.AddMinutes(90)));
    }
}
