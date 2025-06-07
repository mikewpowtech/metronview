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


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SensorDb -> UnitDb (many-to-one)
        modelBuilder.Entity<SensorDb>()
            .HasOne(s => s.Unit)
            .WithMany() // or .WithMany(u => u.Sensors) if you add a collection navigation property
            .HasForeignKey(s => s.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        // SensorDb -> CompanyDb (many-to-one)
        modelBuilder.Entity<SensorDb>()
            .HasOne(s => s.Company)
            .WithMany() // or .WithMany(c => c.Sensors) if you add a collection navigation property
            .HasForeignKey(s => s.CompanyID)
            .OnDelete(DeleteBehavior.Restrict);

        // UnitDb -> CompanyDb (many-to-one)
        modelBuilder.Entity<UnitDb>()
            .HasOne(u => u.Company)
            .WithMany() // or .WithMany(c => c.Units) if you add a collection navigation property
            .HasForeignKey(u => u.CompanyID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
