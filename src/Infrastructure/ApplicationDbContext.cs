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
    // Use new entity for MostRecentReadings
    public DbSet<MostRecentReadingDb> MostRecentReadings { get; set; } = default!;
    public DbSet<UnitModelDb> UnitModels { get; set; } = default!; // Added for UnitModel support
    public DbSet<ConfigurationUploadDb> ConfigurationUploads { get; set; } = default!;
    public DbSet<UnitStatusDb> UnitStatuses { get; set; } = default!;
    // Use new entity for MostRecentUnitStatuses
    public DbSet<MostRecentUnitStatusDb> MostRecentUnitStatuses { get; set; } = default!;
    public DbSet<TriggerDb> Triggers { get; set; } = null!;
    public DbSet<TriggerTypeDb> TriggerTypes { get; set; } = null!;
    public DbSet<CommunicationModeDb> RecipientModes { get; set; } = null!;
    public DbSet<RecipientDb> Recipients { get; set; } = null!;
    public DbSet<RecipientSetDb> RecipientSets { get; set; } = null!;
    public DbSet<AlarmDb> Alarms { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AlarmDb>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Foreign key for Company
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            // Foreign key for RecipientSet
            entity.HasOne(e => e.RecipientSet)
                .WithMany()
                .HasForeignKey(e => e.RecipientSetId)
                .OnDelete(DeleteBehavior.NoAction);

            // Optional one-to-many relationship: AlarmDb -> TriggerDb
            entity.HasMany(e => e.Triggers)
                  .WithOne(e => e.Alarm)
                  .HasForeignKey(e => e.AlarmId)
                  .OnDelete(DeleteBehavior.NoAction)
                  .IsRequired(false); // Triggers are optional
        });

        modelBuilder.Entity<RecipientDb>()
            .HasMany(r => r.RecipientSets)
            .WithMany(rs => rs.Recipients)
            .UsingEntity(j => j.ToTable("RecipientSetRecipients")); // Optional: custom join table name

        modelBuilder.Entity<TriggerDb>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Alarm)
                .WithMany(a => a.Triggers)
                .HasForeignKey(e => e.AlarmId)
                .OnDelete(DeleteBehavior.NoAction);

            // Use TriggerTypeCode as the foreign key to TriggerTypeDb.Code
            entity.HasOne(e => e.TriggerType)
                .WithMany()
                .HasPrincipalKey(tt => tt.Code)
                .HasForeignKey(e => e.TriggerTypeCode)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.CommunicationMode)
                .WithMany()
                .HasForeignKey(e => e.CommunicationModeId)
                .OnDelete(DeleteBehavior.NoAction);

            // Store TriggerTypeCode as char (string of length 1)
            entity.Property(e => e.TriggerTypeCode)
                .HasConversion(
                    v => ((char)v).ToString(),
                    v => (Domain.Enums.TriggerTypeCode)Convert.ToChar(v))
                .HasColumnType("char(1)")
                .IsRequired();
        });

        modelBuilder.Entity<TriggerTypeDb>(entity =>
        {
            // Store Code as char (string of length 1)
            entity.Property(e => e.Code)
                .HasConversion(
                    v => ((char)v).ToString(),
                    v => (Domain.Enums.TriggerTypeCode)Convert.ToChar(v))
                .HasColumnType("char(1)")
                .IsRequired();
        });

        modelBuilder.Entity<ConfigurationUploadDb>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Configure the foreign key relationship
            entity.HasOne(e => e.Unit)
                  .WithMany() // or .WithMany(u => u.Sensors) if you have a collection navigation property in UnitDb
                  .HasForeignKey(e => e.UnitId)
                  .OnDelete(DeleteBehavior.NoAction); // or your preferred delete behavior

            entity.Property(u => u.QueueingUserName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(u => u.DateCreatedUtc)
                .IsRequired();

            entity.Property(u => u.UploadStatusId)
                .IsRequired();
        });

        modelBuilder.Entity<SensorDb>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Foreign key to UnitDb
            entity.HasOne(e => e.Unit)
                  .WithMany(u => u.Sensors)
                  .HasForeignKey(e => e.UnitId)
                  .OnDelete(DeleteBehavior.NoAction);

            // Foreign key to CompanyDb
            entity.HasOne(e => e.Company)
                  .WithMany()
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.NoAction)
                      .IsRequired(false);

            // Foreign key to AlarmDb
            entity.HasOne(e => e.Alarm)
                  .WithMany()
                  .HasForeignKey(e => e.AlarmId)
                  .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
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
            .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
        });


        // ReadingDb configuration (for Readings table)
        modelBuilder.Entity<ReadingDb>(entity =>
        {
            entity.HasKey(r => new { r.DateRecordedUtc, r.SensorId });
            entity.HasOne(r => r.Unit)
                    .WithMany(u => u.Readings)
                    .HasForeignKey(r => r.UnitId)
                    .OnDelete(DeleteBehavior.NoAction);
            // Optionally, configure relationships and properties here
        });

        // MostRecentReadingDb configuration (for MostRecentReadings table)
        modelBuilder.Entity<MostRecentReadingDb>(entity =>
        {
            entity.ToTable("MostRecentReadings");
            entity.HasKey(r => new { r.SensorId });
            entity.HasOne(r => r.Unit)
                    .WithMany()
                    .HasForeignKey(r => r.UnitId)
                    .OnDelete(DeleteBehavior.NoAction);
            entity.HasIndex(r => r.SensorId)
                .IsUnique()
                .HasDatabaseName("IX_MostRecentReadings_SensorId_Unique");
            // Optionally, configure relationships and properties here
        });


        modelBuilder.Entity<UnitStatusDb>(entity =>
        {
            entity.HasKey(r => new { r.DateReceivedUtc, r.UnitId });
            entity.HasOne(r => r.Unit)
                    .WithMany()
                    .HasForeignKey(r => r.UnitId)
                    .OnDelete(DeleteBehavior.NoAction);
            // Optionally, configure relationships and properties here
        });

        // MostRecentUnitStatusDb configuration (for MostRecentUnitStatuses table)
        modelBuilder.Entity<MostRecentUnitStatusDb>(entity =>
        {
            entity.ToTable("MostRecentUnitStatuses");
            entity.HasKey(r => r.UnitId);
            entity.HasOne(r => r.Unit)
                .WithMany()
                .HasForeignKey(r => r.UnitId)
                .OnDelete(DeleteBehavior.NoAction);
            // Add any additional property configs if needed
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
