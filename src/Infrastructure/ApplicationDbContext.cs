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

        modelBuilder.Entity<SensorDb>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Configure the foreign key relationship
            entity.HasOne(e => e.Unit)
                  .WithMany() // or .WithMany(u => u.Sensors) if you have a collection navigation property in UnitDb
                  .HasForeignKey(e => e.UnitId)
                  .OnDelete(DeleteBehavior.Cascade); // or your preferred delete behavior
        });

        modelBuilder.Entity<UnitDb>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id)
                .ValueGeneratedOnAdd(); // Auto-increment primary key

            entity.Property(e => e.Status)
                .HasConversion<string>();

            entity.HasOne(u => u.Company)
            .WithMany()
            .HasForeignKey(u => u.CompanyID)
            .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReadingDb>(entity =>
        {
            entity.HasKey(r => new { r.DateRecordedUtc, r.SensorId });
            // Optionally, configure relationships and properties here
        });

        // UnitModelDb configuration (optional: add constraints if needed)
        modelBuilder.Entity<UnitModelDb>(entity =>
        {
            entity.Property(u => u.Code)
            .HasMaxLength(255)
            .IsRequired();

                        entity.Property(u => u.Name)
            .HasMaxLength(255)
            .IsRequired();
            entity.Property(u => u.Description)
           .HasMaxLength(255);
        });
    }
}
