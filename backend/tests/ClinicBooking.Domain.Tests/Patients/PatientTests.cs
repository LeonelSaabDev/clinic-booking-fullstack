using ClinicBooking.Domain.Common;
using ClinicBooking.Domain.Patients;

namespace ClinicBooking.Domain.Tests.Patients;

public class PatientTests
{
    private static Patient NewPatient() =>
        Patient.Create("Juan", "Pérez", "30123456", "juan@mail.com", "11 5555-1234");

    [Fact]
    public void Create_WithValidData_NormalizesValues()
    {
        var patient = Patient.Create("  Juan  ", "  Pérez  ", " 30.123.456 ", "  Juan@Mail.COM ", " 11 5555-1234 ");

        Assert.NotEqual(Guid.Empty, patient.Id);
        Assert.Equal("Juan", patient.FirstName);
        Assert.Equal("Pérez", patient.LastName);
        Assert.Equal("30123456", patient.DocumentNumber);
        Assert.Equal("juan@mail.com", patient.Email);
        Assert.Equal("11 5555-1234", patient.Phone);
        Assert.Null(patient.HealthInsuranceId);
        Assert.Null(patient.MemberNumber);
    }

    [Theory]
    [InlineData("", "Pérez", "30123456", "juan@mail.com", "1155551234")]
    [InlineData("Juan", " ", "30123456", "juan@mail.com", "1155551234")]
    [InlineData("Juan", "Pérez", "", "juan@mail.com", "1155551234")]
    [InlineData("Juan", "Pérez", "30123456", "", "1155551234")]
    [InlineData("Juan", "Pérez", "30123456", "juan@mail.com", "")]
    public void Create_WithMissingRequiredValue_ThrowsDomainException(
        string firstName, string lastName, string documentNumber, string email, string phone)
    {
        Assert.Throws<DomainException>(() => Patient.Create(firstName, lastName, documentNumber, email, phone));
    }

    [Theory]
    [InlineData("30ABC456")]
    [InlineData("12345")]
    [InlineData("12345678901")]
    public void Create_WithInvalidDocumentNumber_ThrowsDomainException(string documentNumber)
    {
        Assert.Throws<DomainException>(() =>
            Patient.Create("Juan", "Pérez", documentNumber, "juan@mail.com", "1155551234"));
    }

    [Theory]
    [InlineData("juan")]
    [InlineData("juan@")]
    [InlineData("juan mail.com")]
    public void Create_WithInvalidEmail_ThrowsDomainException(string email)
    {
        Assert.Throws<DomainException>(() => Patient.Create("Juan", "Pérez", "30123456", email, "1155551234"));
    }

    [Fact]
    public void Create_WithInvalidPhoneCharacters_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Patient.Create("Juan", "Pérez", "30123456", "juan@mail.com", "call me"));
    }

    [Fact]
    public void UpdateDetails_WithValidData_ChangesValues()
    {
        var patient = NewPatient();

        patient.UpdateDetails("Juana", "Paz", "40111222", "juana@mail.com", "+54 9 11 4444-0000");

        Assert.Equal("Juana", patient.FirstName);
        Assert.Equal("40111222", patient.DocumentNumber);
        Assert.Equal("+54 9 11 4444-0000", patient.Phone);
    }

    [Fact]
    public void SetCoverage_WithHealthInsuranceAndMemberNumber_SetsCoverage()
    {
        var patient = NewPatient();
        var osde = Guid.CreateVersion7();

        patient.SetCoverage(osde, "  210-555  ");

        Assert.Equal(osde, patient.HealthInsuranceId);
        Assert.Equal("210-555", patient.MemberNumber);
    }

    [Fact]
    public void SetCoverage_WithHealthInsuranceButNoMemberNumber_ThrowsDomainException()
    {
        var patient = NewPatient();

        Assert.Throws<DomainException>(() => patient.SetCoverage(Guid.CreateVersion7(), "  "));
    }

    [Fact]
    public void SetCoverage_WithoutHealthInsurance_ClearsMemberNumber()
    {
        var patient = NewPatient();
        patient.SetCoverage(Guid.CreateVersion7(), "210-555");

        patient.SetCoverage(null, "ignored");

        Assert.Null(patient.HealthInsuranceId);
        Assert.Null(patient.MemberNumber);
    }

    [Fact]
    public void SetCoverage_WithEmptyHealthInsuranceId_ThrowsDomainException()
    {
        var patient = NewPatient();

        Assert.Throws<DomainException>(() => patient.SetCoverage(Guid.Empty, "210-555"));
    }
}
