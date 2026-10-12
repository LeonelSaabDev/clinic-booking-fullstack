using ClinicBooking.Domain.Common;
using ClinicBooking.Domain.HealthInsurances;

namespace ClinicBooking.Domain.Tests.HealthInsurances;

public class HealthInsuranceTests
{
    [Fact]
    public void Create_WithValidName_ReturnsHealthInsuranceWithTrimmedName()
    {
        var healthInsurance = HealthInsurance.Create("  OSDE  ");

        Assert.Equal("OSDE", healthInsurance.Name);
        Assert.NotEqual(Guid.Empty, healthInsurance.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ThrowsDomainException(string name)
    {
        Assert.Throws<DomainException>(() => HealthInsurance.Create(name));
    }

    [Fact]
    public void Create_WithNameLongerThanMaxLength_ThrowsDomainException()
    {
        var name = new string('a', HealthInsurance.NameMaxLength + 1);

        Assert.Throws<DomainException>(() => HealthInsurance.Create(name));
    }

    [Fact]
    public void Create_WithNameAtMaxLength_ReturnsHealthInsurance()
    {
        var name = new string('a', HealthInsurance.NameMaxLength);

        var healthInsurance = HealthInsurance.Create(name);

        Assert.Equal(name, healthInsurance.Name);
    }
}
