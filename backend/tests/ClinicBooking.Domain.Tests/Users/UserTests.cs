using ClinicBooking.Domain.Common;
using ClinicBooking.Domain.Users;

namespace ClinicBooking.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void CreateReceptionist_NormalizesEmailAndHasNoDoctor()
    {
        var user = User.CreateReceptionist("  Recepcion@Clinica.COM ", "hashed");

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("recepcion@clinica.com", user.Email);
        Assert.Equal(UserRole.Receptionist, user.Role);
        Assert.Null(user.DoctorId);
        Assert.Equal("hashed", user.PasswordHash);
    }

    [Fact]
    public void CreateDoctorUser_IsLinkedToTheDoctor()
    {
        var doctorId = Guid.CreateVersion7();

        var user = User.CreateDoctorUser("ana@clinica.com", "hashed", doctorId);

        Assert.Equal(UserRole.Doctor, user.Role);
        Assert.Equal(doctorId, user.DoctorId);
    }

    [Fact]
    public void CreateDoctorUser_WithoutDoctor_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => User.CreateDoctorUser("ana@clinica.com", "hashed", Guid.Empty));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("ana@localhost")]
    public void Create_WithInvalidEmail_ThrowsDomainException(string email)
    {
        Assert.Throws<DomainException>(() => User.CreateReceptionist(email, "hashed"));
    }

    [Fact]
    public void Create_WithoutPasswordHash_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => User.CreateReceptionist("recepcion@clinica.com", " "));
    }

    [Fact]
    public void ChangePasswordHash_ReplacesTheHash()
    {
        var user = User.CreateReceptionist("recepcion@clinica.com", "old");

        user.ChangePasswordHash("new");

        Assert.Equal("new", user.PasswordHash);
    }
}
