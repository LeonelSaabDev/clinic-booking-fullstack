using ClinicBooking.Domain.HealthInsurances;
using ClinicBooking.Domain.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicBooking.Infrastructure.Persistence.Configurations;

internal sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(Patient.NameMaxLength);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(Patient.NameMaxLength);
        builder.Property(p => p.DocumentNumber).IsRequired().HasMaxLength(Patient.DocumentNumberMaxLength);
        builder.Property(p => p.Email).IsRequired().HasMaxLength(Patient.EmailMaxLength);
        builder.Property(p => p.Phone).IsRequired().HasMaxLength(Patient.PhoneMaxLength);
        builder.Property(p => p.MemberNumber).HasMaxLength(Patient.MemberNumberMaxLength);

        builder.HasIndex(p => p.DocumentNumber).IsUnique();

        builder.HasOne<HealthInsurance>()
            .WithMany()
            .HasForeignKey(p => p.HealthInsuranceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
