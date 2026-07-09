using Microsoft.EntityFrameworkCore;
namespace RegistrationWebAPI.Data;

public class RegistrationDbContext(DbContextOptions<RegistrationDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();

    public DbSet<OrganisationEntity> Organisations => Set<OrganisationEntity>();

    public DbSet<DownloadAuditEntity> DownloadAudits => Set<DownloadAuditEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<OrganisationEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<DownloadAuditEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserEmail).IsRequired();
            entity.Property(e => e.DownloadType).IsRequired();
            entity.Property(e => e.Version).IsRequired();
            entity.HasIndex(e => e.DownloadedAtUtc);
            entity.HasIndex(e => e.UserEmail);
        });
    }
}
