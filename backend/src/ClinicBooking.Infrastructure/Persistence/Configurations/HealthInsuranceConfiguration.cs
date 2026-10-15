using ClinicBooking.Domain.HealthInsurances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicBooking.Infrastructure.Persistence.Configurations;

internal sealed class HealthInsuranceConfiguration : IEntityTypeConfiguration<HealthInsurance>
{
    public void Configure(EntityTypeBuilder<HealthInsurance> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(HealthInsurance.NameMaxLength);

        builder.HasIndex(h => h.Name).IsUnique();
    }
}
