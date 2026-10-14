using ClinicBooking.Domain.Specialties;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Specialty> Specialties => Set<Specialty>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
