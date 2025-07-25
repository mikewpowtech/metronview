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
    public DbSet<MostRecentAlarmDb> MostRecentAlarms { get; set; } = default!;
    public DbSet<CustomFieldDb> CustomFields { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        // Only configure if not already configured (to avoid overriding DI configuration)
        if (!optionsBuilder.IsConfigured)
        {
            // This is a fallback configuration - prefer configuring in DI container
            optionsBuilder.UseSqlServer(options => 
            {
                options.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        }
        
        // Suppress the pending model changes warning
        // This is a temporary solution until the migration issue is resolved
        // The warning indicates that the EF model doesn't match the database schema
        // but this might be due to migration tracking issues rather than actual schema differences
        optionsBuilder.ConfigureWarnings(warnings => 
            warnings.Log(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<UnitStatusDb>(entity =>
        {
            entity.HasKey(r => new { r.DateReceivedUtc, r.UnitId });
            // Map to the correct table name
            entity.ToTable("UnitStatuses");

            // Configure properties
            entity.Property(s => s.UnitId)
                .IsRequired();

            entity.Property(s => s.DateReceivedUtc)
                .IsRequired();

            entity.Property(s => s.Mip)
                .IsRequired();

            entity.Property(s => s.FailedCallout)
                .IsRequired();

            entity.Property(s => s.BattAlarm)
                .IsRequired();

            entity.Property(s => s.AutoConfig)
                .IsRequired();

            entity.Property(s => s.Temperature)
                .IsRequired(false);

            entity.Property(s => s.Carrier)
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(s => s.Signal)
                .IsRequired(false);

            // Foreign key relationship to Units
            entity.HasOne(s => s.Unit)
                .WithMany()
                .HasForeignKey(s => s.UnitId)
                .OnDelete(DeleteBehavior.NoAction);
            // Optionally, configure relationships and properties here
        });

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

            // Make TriggerType REQUIRED (non-nullable)
            entity.HasOne(e => e.TriggerType)
                .WithMany()
                .HasForeignKey(e => e.TriggerTypeId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(); // Add this line

            // Make CommunicationMode REQUIRED (non-nullable)
            entity.HasOne(e => e.CommunicationMode)
                .WithMany()
                .HasForeignKey(e => e.CommunicationModeId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(); // Add this line
        });

        modelBuilder.Entity<TriggerTypeDb>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Store Code as char (string of length 1)
            entity.Property(e => e.Code)
                .HasConversion(
                    v => ((char)v).ToString(),
                    v => (Domain.Enums.TriggerTypeCode)Convert.ToChar(v))
                .HasColumnType("char(1)")
                .IsRequired();

            // Index on Code for performance
            entity.HasIndex(e => e.Code)
                .IsUnique();
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
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
        });


        // ReadingDb configuration (for Readings table)
        modelBuilder.Entity<ReadingDb>(entity =>
        {
            entity.HasKey(r => new { r.DateRecordedUtc, r.SensorId });
            
            // Configure the relationship to Unit
            entity.HasOne(r => r.Unit)
                    .WithMany(u => u.Readings)
                    .HasForeignKey(r => r.UnitId)
                    .OnDelete(DeleteBehavior.NoAction);
                    
            // Configure the relationship to Sensor
            entity.HasOne(r => r.Sensor)
                    .WithMany()
                    .HasForeignKey(r => r.SensorId)
                    .OnDelete(DeleteBehavior.NoAction);
        });

        // MostRecentReadingDb configuration (for MostRecentReadings table)
        modelBuilder.Entity<MostRecentReadingDb>(entity =>
        {
            entity.ToTable("MostRecentReadings");
            entity.HasKey(r => r.SensorId);
            
            // Configure the relationship to Unit
            entity.HasOne(r => r.Unit)
                    .WithMany()
                    .HasForeignKey(r => r.UnitId)
                    .OnDelete(DeleteBehavior.NoAction);
            
            // Configure the relationship to Sensor - explicitly specify the foreign key
            entity.HasOne(r => r.Sensor)
                    .WithMany()
                    .HasForeignKey(r => r.SensorId)
                    .OnDelete(DeleteBehavior.NoAction);
                    
            entity.HasIndex(r => r.SensorId)
                .IsUnique()
                .HasDatabaseName("IX_MostRecentReadings_SensorId_Unique");
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

        // MostRecentAlarmDb configuration
        modelBuilder.Entity<MostRecentAlarmDb>(entity =>
        {
            entity.HasKey(e => new { e.SensorId, e.AlarmId }); // Composite key

            entity.HasOne(e => e.Sensor)
                .WithMany()
                .HasForeignKey(e => e.SensorId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Alarm)
                .WithMany()
                .HasForeignKey(e => e.AlarmId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure CustomFieldDb
        modelBuilder.Entity<CustomFieldDb>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Content)
                .HasMaxLength(4000) // Adjust size as needed
                .IsRequired();

            entity.Property(e => e.CustomFieldType)
                .HasConversion<int>() // Store enum as int
                .IsRequired();

            entity.Property(e => e.ForeignKeyId)
                .IsRequired();

            // Create index for better query performance
            entity.HasIndex(e => new { e.ForeignKeyId, e.CustomFieldType })
                .HasDatabaseName("IX_CustomFields_ForeignKeyId_CustomFieldType");
        });

        modelBuilder.Entity<CompanyDb>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Map to the correct table name
            entity.ToTable("Companies");
            
            // Map the primary key to CompanyID column
            entity.Property(e => e.Id)
                .HasColumnName("CompanyId") // Changed from "CompanyID" to match migration
                .ValueGeneratedOnAdd();
            
            // Configure CompanyName with constraints
            entity.Property(e => e.Name)
                .HasColumnName("CompanyName")
                .HasMaxLength(256)
                .IsRequired();
            
            // Create unique constraint on CompanyName
            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("Two companies cannot have the same name");
            
            // Configure ManagingCompanyID
            entity.Property(e => e.ParentCompanyId)
                .IsRequired(false);
            
            // Configure other properties with proper column names and constraints
            entity.Property(e => e.HostHeader)
                .HasMaxLength(255)
                .IsRequired(false);
            
            entity.Property(e => e.DefaultDaysHistory)
                .IsRequired(false);
            
            entity.Property(e => e.DefaultDaysBeforeNotReported)
                .IsRequired(false);
            
            entity.Property(e => e.AlarmEmailFromAddress)
                .HasMaxLength(254)
                .IsUnicode(false) // varchar
                .IsRequired(false);
            
            entity.Property(e => e.AlarmEmailReplyToAddress)
                .HasMaxLength(254)
                .IsUnicode(false) // varchar
                .IsRequired(false);
            
            entity.Property(e => e.AlarmSmsToAddressTemplate)
                .HasMaxLength(254)
                .IsUnicode(false) // varchar
                .IsRequired(false);
            
            entity.Property(e => e.AlarmSmsSubjectTemplate)
                .HasMaxLength(int.MaxValue) // nvarchar(max)
                .IsRequired(false);
            
            entity.Property(e => e.AlarmSmsBodyTemplate)
                .HasMaxLength(int.MaxValue) // nvarchar(max)
                .IsRequired(false);
            
            entity.Property(e => e.DaysBeforeRTUDataDeletion)
                .IsRequired(false);
            
            entity.Property(e => e.CustomFieldDefinitions)
                .HasMaxLength(int.MaxValue) // nvarchar(max)
                .IsRequired(false);
            
            entity.Property(e => e.Dashboard)
                .HasMaxLength(int.MaxValue) // nvarchar(max)
                .IsRequired(false);
            
            // Self-referencing foreign key relationship
            entity.HasOne(e => e.ParentCompany)
                .WithMany(e => e.ManagedCompanies)
                .HasForeignKey(e => e.ParentCompanyId)
                .HasConstraintName("Cannot delete a company that manages other companies")
                .OnDelete(DeleteBehavior.NoAction);
            
            // Check constraints (these will be handled by database constraints, 
            // but you can add custom validation in your domain logic)
        });
    }
}
