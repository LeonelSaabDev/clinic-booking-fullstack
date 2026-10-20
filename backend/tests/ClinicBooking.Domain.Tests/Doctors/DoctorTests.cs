using ClinicBooking.Domain.Common;
using ClinicBooking.Domain.Doctors;

namespace ClinicBooking.Domain.Tests.Doctors;

public class DoctorTests
{
    private static readonly Guid SpecialtyId = Guid.CreateVersion7();

    [Fact]
    public void Create_WithValidData_ReturnsDoctorWithTrimmedValues()
    {
        var doctor = Doctor.Create("  Ana  ", "  García  ", "  MN 12345  ", SpecialtyId);

        Assert.NotEqual(Guid.Empty, doctor.Id);
        Assert.Equal("Ana", doctor.FirstName);
        Assert.Equal("García", doctor.LastName);
        Assert.Equal("MN 12345", doctor.LicenseNumber);
        Assert.Equal(SpecialtyId, doctor.SpecialtyId);
        Assert.Empty(doctor.AcceptedHealthInsurances);
    }

    [Theory]
    [InlineData("", "García", "MN 12345")]
    [InlineData("   ", "García", "MN 12345")]
    [InlineData("Ana", "", "MN 12345")]
    [InlineData("Ana", "   ", "MN 12345")]
    [InlineData("Ana", "García", "")]
    [InlineData("Ana", "García", "   ")]
    public void Create_WithMissingRequiredValue_ThrowsDomainException(
        string firstName, string lastName, string licenseNumber)
    {
        Assert.Throws<DomainException>(() => Doctor.Create(firstName, lastName, licenseNumber, SpecialtyId));
    }

    [Fact]
    public void Create_WithFirstNameLongerThanMaxLength_ThrowsDomainException()
    {
        var firstName = new string('a', Doctor.NameMaxLength + 1);

        Assert.Throws<DomainException>(() => Doctor.Create(firstName, "García", "MN 12345", SpecialtyId));
    }

    [Fact]
    public void Create_WithLastNameLongerThanMaxLength_ThrowsDomainException()
    {
        var lastName = new string('a', Doctor.NameMaxLength + 1);

        Assert.Throws<DomainException>(() => Doctor.Create("Ana", lastName, "MN 12345", SpecialtyId));
    }

    [Fact]
    public void Create_WithLicenseNumberLongerThanMaxLength_ThrowsDomainException()
    {
        var licenseNumber = new string('1', Doctor.LicenseNumberMaxLength + 1);

        Assert.Throws<DomainException>(() => Doctor.Create("Ana", "García", licenseNumber, SpecialtyId));
    }

    [Fact]
    public void Create_WithValuesAtMaxLength_ReturnsDoctor()
    {
        var name = new string('a', Doctor.NameMaxLength);
        var licenseNumber = new string('1', Doctor.LicenseNumberMaxLength);

        var doctor = Doctor.Create(name, name, licenseNumber, SpecialtyId);

        Assert.Equal(name, doctor.FirstName);
        Assert.Equal(licenseNumber, doctor.LicenseNumber);
    }

    [Fact]
    public void Create_WithEmptySpecialtyId_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Doctor.Create("Ana", "García", "MN 12345", Guid.Empty));
    }

    [Fact]
    public void UpdateDetails_WithValidData_ChangesValues()
    {
        var doctor = Doctor.Create("Ana", "García", "MN 12345", SpecialtyId);
        var newSpecialtyId = Guid.CreateVersion7();

        doctor.UpdateDetails("Laura", "Pérez", "MP 999", newSpecialtyId);

        Assert.Equal("Laura", doctor.FirstName);
        Assert.Equal("Pérez", doctor.LastName);
        Assert.Equal("MP 999", doctor.LicenseNumber);
        Assert.Equal(newSpecialtyId, doctor.SpecialtyId);
    }

    [Fact]
    public void SetAcceptedHealthInsurances_WithDuplicates_KeepsEachOnlyOnce()
    {
        var doctor = Doctor.Create("Ana", "García", "MN 12345", SpecialtyId);
        var osde = Guid.CreateVersion7();
        var swissMedical = Guid.CreateVersion7();

        doctor.SetAcceptedHealthInsurances([osde, swissMedical, osde]);

        Assert.Equal(2, doctor.AcceptedHealthInsurances.Count);
        Assert.True(doctor.Accepts(osde));
        Assert.True(doctor.Accepts(swissMedical));
    }

    [Fact]
    public void SetAcceptedHealthInsurances_ReplacesPreviousList()
    {
        var doctor = Doctor.Create("Ana", "García", "MN 12345", SpecialtyId);
        var osde = Guid.CreateVersion7();
        var swissMedical = Guid.CreateVersion7();
        doctor.SetAcceptedHealthInsurances([osde]);

        doctor.SetAcceptedHealthInsurances([swissMedical]);

        Assert.False(doctor.Accepts(osde));
        Assert.True(doctor.Accepts(swissMedical));
    }

    [Fact]
    public void SetAcceptedHealthInsurances_WithEmptyList_LeavesDoctorWithoutHealthInsurances()
    {
        var doctor = Doctor.Create("Ana", "García", "MN 12345", SpecialtyId);
        doctor.SetAcceptedHealthInsurances([Guid.CreateVersion7()]);

        doctor.SetAcceptedHealthInsurances([]);

        Assert.Empty(doctor.AcceptedHealthInsurances);
    }

    [Fact]
    public void SetAcceptedHealthInsurances_WithEmptyId_ThrowsDomainException()
    {
        var doctor = Doctor.Create("Ana", "García", "MN 12345", SpecialtyId);

        Assert.Throws<DomainException>(() => doctor.SetAcceptedHealthInsurances([Guid.Empty]));
    }
}
