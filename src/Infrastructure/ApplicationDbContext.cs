using Domain;
using Infrastructure.DbClasses;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUserDb>(options)
{
    //NB the user tables are taken care of by IdentityDbContext<ApplicationUserDb>
    public DbSet<CompanyDb> Companies { get; set; } = default!;
    public DbSet<UnitDb> Units { get; set; } = default!;
    public DbSet<SensorDb> Sensors { get; set; } = default!;
    public DbSet<ReadingDb> Readings { get; set; } = default!;
    public DbSet<UnitModelDb> UnitModels { get; set; } = default!; // Added for UnitModel support

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SensorDb -> UnitDb (many-to-one)
        modelBuilder.Entity<SensorDb>()
            .HasOne(s => s.Unit)
            .WithMany()
            .HasForeignKey(s => s.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UnitDb>(entity =>
        {
            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.Status)
                .HasConversion<string>();

            entity.HasOne(u => u.Company)
            .WithMany()
            .HasForeignKey(u => u.CompanyID)
            .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<ReadingDb>()
            .HasKey(r => new { r.DateRecordedUtc, r.SensorId });

        modelBuilder.Entity<ReadingDb>()
            .HasOne(r => r.Sensor)
            .WithMany()
            .HasForeignKey(r => r.SensorId)
            .OnDelete(DeleteBehavior.Restrict);

        // UnitModelDb configuration (optional: add constraints if needed)
        modelBuilder.Entity<UnitModelDb>()
            .Property(u => u.Code)
            .HasMaxLength(255)
            .IsRequired();

        modelBuilder.Entity<UnitModelDb>()
            .Property(u => u.Name)
            .HasMaxLength(255)
            .IsRequired();

        modelBuilder.Entity<UnitModelDb>()
            .Property(u => u.Description)
            .HasMaxLength(255);
    }
}
