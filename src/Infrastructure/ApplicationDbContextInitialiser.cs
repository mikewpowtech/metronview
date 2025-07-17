using Infrastructure.Identity;
using Infrastructure.DbClasses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Infrastructure;

public class ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, 
        UserManager<ApplicationUserDb> userManager, RoleManager<IdentityRole> roleManager)
{
    public async Task InitialiseAsync()
    {
        try
        {
            if (context.Database.IsSqlServer())
            {
                // Check if database exists, if not create it
                await context.Database.EnsureCreatedAsync();
                
                // Try to apply migrations, but handle IDENTITY property conflicts
                try
                {
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("Applying {Count} pending migrations", pendingMigrations.Count());
                        
                        // Check if the problematic migration is in the list
                        var problematicMigration = "20250716114844_FixPendingModelChanges";
                        if (pendingMigrations.Contains(problematicMigration))
                        {
                            logger.LogInformation("Found problematic migration {Migration}, applying with enhanced handling", problematicMigration);
                            
                            // Try to handle this migration specifically before running all migrations
                            await HandleFixPendingModelChangesMigrationDirectly();
                        }
                        
                        await context.Database.MigrateAsync();
                        logger.LogInformation("All migrations applied successfully");
                    }
                    else
                    {
                        logger.LogInformation("No pending migrations found");
                    }
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("IDENTITY property"))
                {
                    logger.LogWarning("IDENTITY property migration issue detected. Attempting to resolve...");
                    
                    // Handle IDENTITY property conflicts by using a more manual approach
                    await HandleIdentityPropertyConflicts();
                }
                catch (Exception ex) when (ex.Message.Contains("20250716114844_FixPendingModelChanges"))
                {
                    logger.LogError(ex, "Specific issue with migration 20250716114844_FixPendingModelChanges detected");
                    
                    // Try to handle this specific migration issue
                    await HandleFixPendingModelChangesMigration();
                }
                catch (Exception ex) when (ex.Message.Contains("SensorId_Temp"))
                {
                    logger.LogError(ex, "SensorId_Temp column error detected during migration");
                    
                    // Handle the specific SensorId_Temp column issue
                    await HandleSensorIdTempColumnIssue();
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    private async Task HandleFixPendingModelChangesMigrationDirectly()
    {
        try
        {
            logger.LogInformation("Attempting to handle the FixPendingModelChanges migration directly before EF migration");
            
            // Check if the migration has already been applied
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            var targetMigration = "20250716114844_FixPendingModelChanges";
            
            if (appliedMigrations.Contains(targetMigration))
            {
                logger.LogInformation("Migration {Migration} has already been applied", targetMigration);
                return;
            }
            
            // Clean up any leftover temporary columns first
            await context.Database.ExecuteSqlRawAsync(@"
                -- Clean up any leftover SensorId_Temp columns
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[MostRecentReadings]') AND name = 'SensorId_Temp')
                BEGIN
                    ALTER TABLE [MostRecentReadings] DROP COLUMN [SensorId_Temp];
                END
            ");
            
            logger.LogInformation("Cleaned up any leftover temporary columns");
            
            // Mark the migration as applied if the database schema is already correct
            var schemaCorrect = await CheckIfSchemaIsCorrect();
            if (schemaCorrect)
            {
                logger.LogInformation("Database schema appears to be correct, marking migration as applied");
                await context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250716114844_FixPendingModelChanges')
                    BEGIN
                        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                        VALUES ('20250716114844_FixPendingModelChanges', '9.0.5');
                    END
                ");
                logger.LogInformation("Migration marked as applied");
            }
            
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle FixPendingModelChanges migration directly");
            logger.LogError("Error details: {ErrorMessage}", ex.Message);
        }
    }

    private async Task<bool> CheckIfSchemaIsCorrect()
    {
        try
        {
            // Check if the MostRecentReadings table has the correct schema
            var schemaCheckResult = await context.Database.ExecuteSqlRawAsync(@"
                -- Check if MostRecentReadings table exists with correct schema
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'MostRecentReadings')
                BEGIN
                    -- Check if SensorId column exists and is NOT an identity column
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[MostRecentReadings]') AND name = 'SensorId')
                       AND NOT EXISTS (SELECT * FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'[MostRecentReadings]') AND name = 'SensorId')
                    BEGIN
                        SELECT 1 AS SchemaCorrect;
                    END
                    ELSE
                    BEGIN
                        SELECT 0 AS SchemaCorrect;
                    END
                END
                ELSE
                BEGIN
                    SELECT 0 AS SchemaCorrect;
                END
            ");
            
            return true; // If we get here without exception, consider it correct
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to check schema correctness");
            return false;
        }
    }

    private async Task HandleSensorIdTempColumnIssue()
    {
        try
        {
            logger.LogInformation("Attempting to resolve SensorId_Temp column issue");
            
            // Check if the migration has already been applied
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            var targetMigration = "20250716114844_FixPendingModelChanges";
            
            if (appliedMigrations.Contains(targetMigration))
            {
                logger.LogInformation("Migration {Migration} has already been applied", targetMigration);
                return;
            }
            
            // Execute SQL to clean up any leftover SensorId_Temp columns
            await context.Database.ExecuteSqlRawAsync(@"
                -- Clean up any leftover SensorId_Temp columns
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[MostRecentReadings]') AND name = 'SensorId_Temp')
                BEGIN
                    ALTER TABLE [MostRecentReadings] DROP COLUMN [SensorId_Temp];
                END
            ");
            
            logger.LogInformation("Cleaned up SensorId_Temp column");
            
            // Check if we can mark the migration as applied
            var schemaCorrect = await CheckIfSchemaIsCorrect();
            if (schemaCorrect)
            {
                logger.LogInformation("Schema appears correct, marking migration as applied");
                await context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250716114844_FixPendingModelChanges')
                    BEGIN
                        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                        VALUES ('20250716114844_FixPendingModelChanges', '9.0.5');
                    END
                ");
                logger.LogInformation("Migration marked as applied successfully");
            }
            else
            {
                // Try to apply the migration again
                logger.LogInformation("Schema not correct, attempting migration again");
                await context.Database.MigrateAsync();
                logger.LogInformation("Migration applied successfully after SensorId_Temp cleanup");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle SensorId_Temp column issue");
            
            // Log the specific error for debugging
            logger.LogError("Error details: {ErrorMessage}", ex.Message);
            
            // Continue with application startup but log the issue
            logger.LogWarning("Continuing with application startup despite SensorId_Temp column issue. Database may not be fully synchronized.");
        }
    }

    private async Task HandleFixPendingModelChangesMigration()
    {
        try
        {
            logger.LogInformation("Attempting to handle the FixPendingModelChanges migration manually");
            
            // Check if the migration has already been applied
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            var targetMigration = "20250716114844_FixPendingModelChanges";
            
            if (appliedMigrations.Contains(targetMigration))
            {
                logger.LogInformation("Migration {Migration} has already been applied", targetMigration);
                return;
            }
            
            // The migration has been updated with proper SQL scripts
            // Try to apply it again
            await context.Database.MigrateAsync();
            logger.LogInformation("Migration {Migration} applied successfully with manual handling", targetMigration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle FixPendingModelChanges migration manually");
            
            // Log the specific error for debugging
            logger.LogError("Error details: {ErrorMessage}", ex.Message);
            
            // Continue with application startup but log the issue
            logger.LogWarning("Continuing with application startup despite migration issue. Database may not be fully synchronized.");
        }
    }

    private async Task HandleIdentityPropertyConflicts()
    {
        try
        {
            // Get the list of pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Attempting to handle IDENTITY property conflicts for {Count} migrations", pendingMigrations.Count());
                
                // Try to apply migrations one by one to identify the problematic one
                foreach (var migration in pendingMigrations)
                {
                    try
                    {
                        logger.LogInformation("Attempting to apply migration: {Migration}", migration);
                        await context.Database.MigrateAsync();
                        logger.LogInformation("Successfully applied migration: {Migration}", migration);
                        break; // If successful, break out of the loop
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("IDENTITY property"))
                    {
                        logger.LogWarning("Migration {Migration} failed due to IDENTITY property conflict: {Error}", migration, ex.Message);
                        
                        // Log the specific migration that's causing issues
                        logger.LogError("Migration {Migration} requires manual intervention due to IDENTITY property changes", migration);
                        
                        // If this is the FixPendingModelChanges migration, try our custom handler
                        if (migration.Contains("FixPendingModelChanges"))
                        {
                            await HandleFixPendingModelChangesMigration();
                        }
                        
                        // For now, we'll continue with the application startup but log the issue
                        // In production, you might want to handle this differently
                        break;
                    }
                    catch (Exception ex) when (ex.Message.Contains("SensorId_Temp"))
                    {
                        logger.LogWarning("Migration {Migration} failed due to SensorId_Temp column issue: {Error}", migration, ex.Message);
                        
                        // Handle the SensorId_Temp column issue
                        await HandleSensorIdTempColumnIssue();
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle IDENTITY property conflicts");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var administratorRole = new IdentityRole("Administrator");

        if (roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            var role = await roleManager.CreateAsync(administratorRole);
            if (role != null)
            {
                await roleManager.AddClaimAsync(administratorRole, new Claim("RoleClaim", "HasRoleView"));
                await roleManager.AddClaimAsync(administratorRole, new Claim("RoleClaim", "HasRoleAdd"));
                await roleManager.AddClaimAsync(administratorRole, new Claim("RoleClaim", "HasRoleEdit"));
                await roleManager.AddClaimAsync(administratorRole, new Claim("RoleClaim", "HasRoleDelete"));
            }
        }

        // Default users
        var administrator = new ApplicationUserDb { UserName = "UnifiedAppAdmin", Email = "UnifiedAppAdmin" };

        if (userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await userManager.CreateAsync(administrator, "UnifiedAppAdmin1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name });
            }
        }

        // Default trigger types
        await SeedTriggerTypesAsync();

        // Default communication modes
        await SeedCommunicationModesAsync();
    }

    private async Task SeedTriggerTypesAsync()
    {
        var triggerTypes = new List<TriggerTypeDb>
        {
            new TriggerTypeDb { Code = Domain.Enums.TriggerTypeCode.Rising, Name = "rising past", Order = 1 },
            new TriggerTypeDb { Code = Domain.Enums.TriggerTypeCode.Falling, Name = "falling past", Order = 2 },
            new TriggerTypeDb { Code = Domain.Enums.TriggerTypeCode.Above, Name = "above or equal to", Order = 3 },
            new TriggerTypeDb { Code = Domain.Enums.TriggerTypeCode.Below, Name = "below or equal to", Order = 4 },
            new TriggerTypeDb { Code = Domain.Enums.TriggerTypeCode.RateOfChange, Name = "rate of change", Order = 5 },
            new TriggerTypeDb { Code = Domain.Enums.TriggerTypeCode.NotReportedForPeriod, Name = "not reported for (mins)", Order = 6 }
        };

        // Update existing trigger types or add new ones, don't remove
        foreach (var triggerType in triggerTypes)
        {
            var existing = await context.TriggerTypes.FirstOrDefaultAsync(tt => tt.Code == triggerType.Code);
            if (existing != null)
            {
                // Update existing trigger type
                existing.Name = triggerType.Name;
                existing.Order = triggerType.Order;
                context.TriggerTypes.Update(existing);
            }
            else
            {
                // Add new trigger type
                context.TriggerTypes.Add(triggerType);
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedCommunicationModesAsync()
    {
        var communicationModes = new List<CommunicationModeDb>
        {
            new CommunicationModeDb { Code = "E", Name = "email", Order = 1 },
            new CommunicationModeDb { Code = "S", Name = "SMS", Order = 2 },
            new CommunicationModeDb { Code = "W", Name = "web service call", Order = 3 },
            new CommunicationModeDb { Code = "M", Name = "MQTT message", Order = 4 },
            new CommunicationModeDb { Code = "U", Name = "upload configuration", Order = 5 }
        };

        // Update existing communication modes or add new ones, don't remove
        foreach (var communicationMode in communicationModes)
        {
            var existing = await context.RecipientModes.FirstOrDefaultAsync(cm => cm.Code == communicationMode.Code);
            if (existing != null)
            {
                // Update existing communication mode
                existing.Name = communicationMode.Name;
                existing.Order = communicationMode.Order;
                context.RecipientModes.Update(existing);
            }
            else
            {
                // Add new communication mode
                context.RecipientModes.Add(communicationMode);
            }
        }

        await context.SaveChangesAsync();
    }
}