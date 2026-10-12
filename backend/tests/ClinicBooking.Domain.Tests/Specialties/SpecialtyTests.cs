using ClinicBooking.Domain.Common;
using ClinicBooking.Domain.Specialties;

namespace ClinicBooking.Domain.Tests.Specialties;

public class SpecialtyTests
{
    [Fact]
    public void Create_WithValidName_ReturnsSpecialtyWithTrimmedName()
    {
        var specialty = Specialty.Create("  Cardiology  ");

        Assert.Equal("Cardiology", specialty.Name);
        Assert.NotEqual(Guid.Empty, specialty.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ThrowsDomainException(string name)
    {
        Assert.Throws<DomainException>(() => Specialty.Create(name));
    }

    [Fact]
    public void Create_WithNameLongerThanMaxLength_ThrowsDomainException()
    {
        var name = new string('a', Specialty.NameMaxLength + 1);

        Assert.Throws<DomainException>(() => Specialty.Create(name));
    }

    [Fact]
    public void Create_WithNameAtMaxLength_ReturnsSpecialty()
    {
        var name = new string('a', Specialty.NameMaxLength);

        var specialty = Specialty.Create(name);

        Assert.Equal(name, specialty.Name);
    }
}
