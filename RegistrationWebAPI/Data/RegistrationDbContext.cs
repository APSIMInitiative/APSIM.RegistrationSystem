using Microsoft.EntityFrameworkCore;

namespace APSIM.RegistrationAPIV2.Data;

public class RegistrationDbContext(DbContextOptions<RegistrationDbContext> options) : DbContext(options)
{
    public DbSet<RegistrationEntity> Registrations => Set<RegistrationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RegistrationEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ContactName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ContactEmail).HasMaxLength(320).IsRequired();
            entity.Property(e => e.OrganisationName).HasMaxLength(300);
            entity.Property(e => e.OrganisationAddress).HasMaxLength(500);
            entity.Property(e => e.OrganisationWebsite).HasMaxLength(500);
            entity.Property(e => e.ContactPhone).HasMaxLength(50);
            entity.HasIndex(e => e.ContactEmail);
            entity.HasIndex(e => e.ApplicationDate);
        });
    }
}
