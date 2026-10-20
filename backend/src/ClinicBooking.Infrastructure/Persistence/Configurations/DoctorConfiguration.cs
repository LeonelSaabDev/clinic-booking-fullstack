using ClinicBooking.Domain.Doctors;
using ClinicBooking.Domain.HealthInsurances;
using ClinicBooking.Domain.Specialties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicBooking.Infrastructure.Persistence.Configurations;

internal sealed class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.FirstName)
            .IsRequired()
            .HasMaxLength(Doctor.NameMaxLength);

        builder.Property(d => d.LastName)
            .IsRequired()
            .HasMaxLength(Doctor.NameMaxLength);

        builder.Property(d => d.LicenseNumber)
            .IsRequired()
            .HasMaxLength(Doctor.LicenseNumberMaxLength);

        builder.HasIndex(d => d.LicenseNumber).IsUnique();

        builder.HasOne<Specialty>()
            .WithMany()
            .HasForeignKey(d => d.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(d => d.AcceptedHealthInsurances, accepted =>
        {
            accepted.ToTable("DoctorHealthInsurances");
            accepted.WithOwner().HasForeignKey("DoctorId");
            accepted.HasKey("DoctorId", nameof(AcceptedHealthInsurance.HealthInsuranceId));

            accepted.HasOne<HealthInsurance>()
                .WithMany()
                .HasForeignKey(a => a.HealthInsuranceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
