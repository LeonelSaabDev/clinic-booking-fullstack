using ClinicBooking.Domain.Appointments;
using ClinicBooking.Domain.Doctors;
using ClinicBooking.Domain.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicBooking.Infrastructure.Persistence.Configurations;

internal sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public const string DoctorSlotIndexName = "IX_Appointments_DoctorId_StartsAt_Scheduled";

    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(a => a.MemberNumber).HasMaxLength(Patient.MemberNumberMaxLength);

        builder.HasOne<Doctor>()
            .WithMany()
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // The final guard against double booking: at most one *scheduled* appointment per doctor and start.
        // Cancelled or past appointments are filtered out, so they never block the slot.
        builder.HasIndex(a => new { a.DoctorId, a.StartsAt })
            .IsUnique()
            .HasFilter("[Status] = N'Scheduled'")
            .HasDatabaseName(DoctorSlotIndexName);

        builder.HasIndex(a => new { a.PatientId, a.StartsAt });
        builder.HasIndex(a => a.StartsAt);
    }
}
